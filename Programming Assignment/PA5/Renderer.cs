using ILGPU;
using ILGPU.Runtime;
using ILGPU.Util;
using Maths;
using PA.Graphics;

namespace PA5;

internal unsafe class Renderer : IDisposable
{
    private readonly Scene _scene;
    private readonly bool _useCpu;

    private Context? context;
    private Accelerator? accelerator;
    private Action<Index2D, Int2, ArrayView1D<Geometry, Stride1D.Dense>, ArrayView1D<Light, Stride1D.Dense>, ArrayView1D<Vector4d, Stride1D.Dense>>? renderKernel;

    private int currentWidth;
    private int currentHeight;
    private MemoryBuffer1D<Geometry, Stride1D.Dense>? objectsBuffer;
    private MemoryBuffer1D<Light, Stride1D.Dense>? lightsBuffer;
    private MemoryBuffer1D<Vector4d, Stride1D.Dense>? frameBuffer;

    public Renderer(Scene scene, bool useCpu = false)
    {
        _scene = scene;
        _useCpu = useCpu;

        InitializeILGPU();
    }

    #region Scene Properties
    public int Width => _scene.Width;

    public int Height => _scene.Height;

    public FrameBuffer FrameBuffer => _scene.FrameBuffer;
    #endregion

    public void Render()
    {
        if (!UpdateBuffers())
        {
            return;
        }

        renderKernel!(new Index2D(currentWidth, currentHeight),
                      new Int2(currentWidth, currentHeight),
                      objectsBuffer!,
                      lightsBuffer!,
                      frameBuffer!);

        accelerator!.Synchronize();

        Span<Vector4d> colorBuffer = new(FrameBuffer.FinalColorBuffer, FrameBuffer.BufferSize);

        frameBuffer!.View.CopyToCPU(ref colorBuffer[0], colorBuffer.Length);

        FrameBuffer.UpdateTexture();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private void InitializeILGPU()
    {
        context = Context.CreateDefault();

        List<Device> devices = [.. context];

        Device? useDevice = null;
        if (!_useCpu)
        {
            if (devices.FirstOrDefault(item => item.AcceleratorType == AcceleratorType.Cuda) is Device device1)
            {
                useDevice = device1;
            }
            else if (devices.FirstOrDefault(item => item.AcceleratorType == AcceleratorType.OpenCL) is Device device2)
            {
                useDevice = device2;
            }
        }

        useDevice ??= devices.First(item => item.AcceleratorType == AcceleratorType.CPU);

        accelerator = useDevice.CreateAccelerator(context);

        renderKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, Int2, ArrayView1D<Geometry, Stride1D.Dense>, ArrayView1D<Light, Stride1D.Dense>, ArrayView1D<Vector4d, Stride1D.Dense>>(RenderKernel);
    }

    private bool UpdateBuffers()
    {
        if (accelerator == null)
        {
            return false;
        }

        objectsBuffer?.Dispose();
        lightsBuffer?.Dispose();
        objectsBuffer = accelerator.Allocate1D<Geometry>(_scene.Objects.Count);
        lightsBuffer = accelerator.Allocate1D<Light>(_scene.Lights.Count);

        if (currentWidth != Width || currentHeight != Height)
        {
            frameBuffer?.Dispose();

            currentWidth = Width;
            currentHeight = Height;

            frameBuffer = accelerator.Allocate1D<Vector4d>(currentWidth * currentHeight);
        }

        objectsBuffer.CopyFromCPU([.. _scene.Objects]);
        lightsBuffer.CopyFromCPU([.. _scene.Lights]);
        frameBuffer!.MemSetToZero();

        return true;
    }

    private static void RenderKernel(Index2D index,
                                     Int2 size,
                                     ArrayView1D<Geometry, Stride1D.Dense> objects,
                                     ArrayView1D<Light, Stride1D.Dense> lights,
                                     ArrayView1D<Vector4d, Stride1D.Dense> frame)
    {
        frame[index.Y * size.X + index.X] = new Vector4d(1, 0, 0, 1);
    }
}
