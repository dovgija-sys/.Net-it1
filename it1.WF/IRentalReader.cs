using it1.DB;

namespace it1.WF
{
    public interface IRentalReader
    {
        IAsyncEnumerable<HostDto> ReadAsync(string path, CancellationToken ct = default);
    }
}
