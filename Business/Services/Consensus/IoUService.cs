namespace DataLabelProject.Business.Services.Consensus;

public interface IIoUService
{
	double Calculate(BoxCandidate a, BoxCandidate b);
}

public class IoUService : IIoUService
{
	public double Calculate(BoxCandidate a, BoxCandidate b)
	{
		var ax1 = a.X;
		var ay1 = a.Y;
		var ax2 = a.X + a.Width;
		var ay2 = a.Y + a.Height;

		var bx1 = b.X;
		var by1 = b.Y;
		var bx2 = b.X + b.Width;
		var by2 = b.Y + b.Height;

		var interW = Math.Max(0, Math.Min(ax2, bx2) - Math.Max(ax1, bx1));
		var interH = Math.Max(0, Math.Min(ay2, by2) - Math.Max(ay1, by1));
		var interArea = interW * interH;

		var areaA = a.Width * a.Height;
		var areaB = b.Width * b.Height;
		var union = areaA + areaB - interArea;

		return union <= 0 ? 0 : interArea / union;
	}
}
