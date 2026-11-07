using it1.DB;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace it1.WF
{
    public class JsonRentalReader : IRentalReader
    {
        private static readonly JsonSerializerOptions Opt = new(JsonSerializerDefaults.Web);

        public async IAsyncEnumerable<HostDto> ReadAsync(
            string path, [EnumeratorCancellation] CancellationToken ct = default)
        {
            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read,
                                                FileShare.Read, 4096, useAsync: true);
            await foreach (var h in JsonSerializer.DeserializeAsyncEnumerable<HostDto>(fs, Opt, ct))
                if (h != null) yield return h;
        }
    }
}
