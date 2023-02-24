// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Serilog;
using System.Collections;
using System.Text;

namespace RapiBarFetch;

public class BarSet : IEnumerable<Bar>
{
    private readonly List<Bar> bars = new();

    public BarSet(Job job)
    {
        Job = job;

    }

    public Job Job { get; }

    public void Add(Bar tick)
    {
        if (bars.Count > 0 && tick.CloseOn < bars[^1].CloseOn)
            throw new ArgumentNullException(nameof(tick.CloseOn));

        bars.Add(tick);
    }

    public int Count => bars.Count;

    public Bar this[int index] => bars[index];

    public override string ToString() => Job.GetFileName(BarKind.CSV);

    public void Save(ILogger logger, string saveToPath, bool inFolders)
    {
        foreach (var barKind in Job.BarKinds)
        {
            var path = Job.GetFullPath(saveToPath, inFolders, barKind);

            path.EnsureFolderExists();

            var sb = new StringBuilder();

            foreach (var bar in bars)
            {
                if (barKind == BarKind.CSV)
                    sb.AppendLine(bar.ToCsvString());
                else
                    sb.AppendLine(bar.ToNinjaString());
            }

            File.WriteAllText(path, sb.ToString());

            var fileName = Job.GetFileName(barKind);

            logger.Information($"BarSetSaved: {fileName} ({Count:N0} Bars)");
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<Bar> GetEnumerator() => bars.GetEnumerator();
}