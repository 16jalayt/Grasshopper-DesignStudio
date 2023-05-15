namespace Cricut_Design_Studio
{
	public class Vertex
	{
		public int x;

		public int y;

		public Vertex next;

		public Vertex prev;

		public bool intersect;

		public bool entry;

		public Vertex neighbor;

		public float alpha;

		public Vertex nextPoly;

		public bool visited;

		public int holes;

		public Vertex()
		{
		}

		public Vertex(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}
}
