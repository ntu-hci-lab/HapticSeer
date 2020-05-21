using System.Drawing;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public class BitmapBuffer
{
    private BufferBlock<Bitmap> UnusedBuffer = new BufferBlock<Bitmap>();
    private BufferBlock<Bitmap> ProcessingBuffer = new BufferBlock<Bitmap>();

    public void PushUnusedBitmap(Bitmap UnusedBitmap)
    {
        if (UnusedBitmap != null && !UnusedBitmap.Size.IsEmpty)
            UnusedBuffer.Post(UnusedBitmap);
    }

    public Bitmap GetUnusedBitmap()
    {
        if (UnusedBuffer.Count == 0)
            return null;
        return UnusedBuffer.Receive();
    }

    public void PushProcessingBitmap(Bitmap ProcessingBitmap)
    {
        ProcessingBuffer.Post(ProcessingBitmap);
    }

    public async Task<Bitmap> GetProcessingBitmap()
    {
        await ProcessingBuffer.OutputAvailableAsync().ConfigureAwait(false);
        return ProcessingBuffer.Receive();
    }
}