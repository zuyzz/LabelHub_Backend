namespace DataLabelProject.Business.Services.Consensus;

public interface IClusteringService
{
	List<BoxCluster> ClusterByIoU(List<BoxCandidate> boxes, double iouThreshold);
}

public class ClusteringService : IClusteringService
{
	private readonly IIoUService _ioUService;

	public ClusteringService(IIoUService ioUService)
	{
		_ioUService = ioUService;
	}

	public List<BoxCluster> ClusterByIoU(List<BoxCandidate> boxes, double iouThreshold)
	{
		if (boxes.Count == 0)
			return [];

		var parent = Enumerable.Range(0, boxes.Count).ToArray();

		int Find(int x)
		{
			if (parent[x] != x)
				parent[x] = Find(parent[x]);
			return parent[x];
		}

		void Union(int a, int b)
		{
			var ra = Find(a);
			var rb = Find(b);
			if (ra != rb)
				parent[rb] = ra;
		}

		for (var i = 0; i < boxes.Count; i++)
		{
			for (var j = i + 1; j < boxes.Count; j++)
			{
				var iou = _ioUService.Calculate(boxes[i], boxes[j]);
				if (iou >= iouThreshold)
					Union(i, j);
			}
		}

		var grouped = new Dictionary<int, BoxCluster>();
		for (var i = 0; i < boxes.Count; i++)
		{
			var root = Find(i);
			if (!grouped.TryGetValue(root, out var cluster))
			{
				cluster = new BoxCluster();
				grouped[root] = cluster;
			}

			cluster.Members.Add(boxes[i]);
		}

		return grouped.Values.ToList();
	}
}
