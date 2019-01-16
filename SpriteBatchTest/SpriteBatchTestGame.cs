#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

public static class Extensions
{
	public static double StdDev(this IEnumerable<double> values)
	{
		double ret = 0;
		int count = values.Count();
		if (count > 1)
		{
			//Compute the Average
			double avg = values.Average();

			//Perform the Sum of (value-avg)^2
			double sum = values.Sum(d => (d - avg) * (d - avg));

			//Put it all together
			ret = Math.Sqrt(sum / count);
		}

		return ret;
	}
}

class SimpleRunningAverage
{
	readonly int _size;
	readonly double[] _values = null;
	int _valuesIndex = 0;
	int _valueCount = 0;
	double _sum = 0;

	public SimpleRunningAverage(int size)
	{
		Debug.Assert(size > 0);
		_size = Math.Max(size, 1);
		_values = new double[_size];
	}

	public double Add(double newValue)
	{
		// calculate new value to add to sum by subtracting the
		// value that is replaced from the new value;
		double temp = newValue - _values[_valuesIndex];
		_values[_valuesIndex] = newValue;
		_sum += temp;

		_valuesIndex++;
		_valuesIndex %= _size;

		if (_valueCount < _size)
			_valueCount++;

		return _sum / _valueCount;
	}
}

class Stat
{
	public readonly SimpleRunningAverage Average2 = new SimpleRunningAverage(5 * 60);
	public double Average;
	public double Max = double.MinValue;
	public double Min = double.MaxValue;
	public double Median;

	public void Update(double value)
	{
		Max = Math.Max(Max, value);
		Min = Math.Min(Min, value);
		Median = (Min + Max) / 2;
		Average = Average2.Add(value);
	}
}

#if !FNA
public enum SpriteBatchImpl
{
	Original,
	OriginalInlinedCompare,
	SortUsingIndex,
	SortUsingInfoIndex,
	SpriteInfo
}
#endif

class SpriteBatchTestGame : Game
{
	private const int SPRITECOUNT = 2048;
	private const int TEXTURECOUNT = 2;

	private const int TEXTURESIZE = 128;
	private static readonly Color TEXTURECOLOR = Color.White;

	private SpriteSortMode mode;

#if FNA
  private SpriteBatchImpl? impl { get { return SpriteBatch.impl; } set { SpriteBatch.impl = value; } }
#else
	private SpriteBatchImpl? impl { get; set; }
#endif

	struct TestMode
	{
		public readonly SpriteBatchImpl Implementation;
		public readonly List<SpriteSortMode> SortModes;

		public TestMode(SpriteBatchImpl implementation, List<SpriteSortMode> sortModes)
		{
			Implementation = implementation;
			SortModes = sortModes;
		}
	}

	private readonly List<TestMode> TestModes;

	private readonly Stopwatch timer;
	private Random random;
	private readonly Vector2[] positions;
	private readonly float[] depths;
	private readonly Color[] colors;
	private readonly Texture2D[] boxRefs;
	private Texture2D[] boxes;
	private SpriteBatch batch;

	public SpriteBatchTestGame()
	{
		new GraphicsDeviceManager(this);
		timer = new Stopwatch();
		random = new Random(0);
		positions = new Vector2[SPRITECOUNT];
		depths = new float[SPRITECOUNT];
		colors = new Color[SPRITECOUNT];
		boxRefs = new Texture2D[SPRITECOUNT];

		List<SpriteSortMode> sortModes = new List<SpriteSortMode>
		{
			SpriteSortMode.Deferred,
			SpriteSortMode.Immediate,
			SpriteSortMode.Texture,
			SpriteSortMode.BackToFront,
			SpriteSortMode.FrontToBack
		};

#if FNA
		TestModes = new List<TestMode>
    {
	    new TestMode(SpriteBatchImpl.Original, sortModes),
	    new TestMode(SpriteBatchImpl.OriginalInlinedCompare, sortModes),
	    new TestMode(SpriteBatchImpl.SortUsingIndex, sortModes),
	    new TestMode(SpriteBatchImpl.SortUsingInfoIndex, sortModes),
	    new TestMode(SpriteBatchImpl.SpriteInfo, sortModes),
	    new TestMode(SpriteBatchImpl.SpriteInfoNonInlined, sortModes),
	    new TestMode(SpriteBatchImpl.Flibit, sortModes)
    };
#else
		TestModes = new List<TestMode> {new TestMode(SpriteBatchImpl.Original, sortModes)};
#endif

		mode = TestModes[0].SortModes[0];
		impl = TestModes[0].Implementation;
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		timer.Start();
		batch.Begin(mode, BlendState.AlphaBlend);
		for (int i = 0; i < SPRITECOUNT; i += 1)
		{
			batch.Draw(boxRefs[i], positions[i], null, colors[i], 0, Vector2.Zero, 1, SpriteEffects.None, depths[i]);
		}

		batch.End();
		timer.Stop();
		if (elapsed > .5f)
		{
			stats.TryGetValue(mode, out Stat stat2);
			if (stat2 == null)
			{
				stat2 = new Stat();
				stats[mode] = stat2;
			}

			stat2.Update(timer.ElapsedTicks);
		}

		timer.Reset();
	}

	private readonly Dictionary<SpriteSortMode, Stat> stats = new Dictionary<SpriteSortMode, Stat>();

	private KeyboardState currentKeyboardState;
	private KeyboardState previousKeyboardState;

	private bool IsKeyPressed(Keys key)
	{
		return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
	}

	private double elapsed;

	private int currentSortOrder;
	private int currentImplementationIndex;

	protected override void Update(GameTime gameTime)
	{
		elapsed += gameTime.ElapsedGameTime.TotalSeconds;

		if (elapsed > 1.5f)
		{
			if (currentImplementationIndex >= TestModes.Count)
			{
				Exit();
			}
			else
			{
				if (currentSortOrder >= TestModes[currentImplementationIndex].SortModes.Count)
				{
					WriteStats();
					++currentImplementationIndex;
					currentSortOrder = 0;
					stats.Clear();

					if (currentImplementationIndex < TestModes.Count)
					{
						mode = TestModes[currentImplementationIndex].SortModes[currentSortOrder];
						impl = TestModes[currentImplementationIndex].Implementation;
					}
				}
				else
				{
					random = new Random(0);

					++currentSortOrder;
					if (currentSortOrder < TestModes[currentImplementationIndex].SortModes.Count)
					{
						mode = TestModes[currentImplementationIndex].SortModes[currentSortOrder];
					}
				}

				elapsed = 0;
			}
		}

		currentKeyboardState = Keyboard.GetState();

		if (IsKeyPressed(Keys.D))
		{
			WriteStats();
		}

		if (IsKeyPressed(Keys.D1))
		{
			mode = SpriteSortMode.Deferred;
		}
		else if (IsKeyPressed(Keys.D2))
		{
			mode = SpriteSortMode.Immediate;
		}
		else if (IsKeyPressed(Keys.D3))
		{
			mode = SpriteSortMode.Texture;
		}
		else if (IsKeyPressed(Keys.D4))
		{
			mode = SpriteSortMode.BackToFront;
		}
		else if (IsKeyPressed(Keys.D5))
		{
			mode = SpriteSortMode.FrontToBack;
		}

		for (int i = 0; i < SPRITECOUNT; i += 1)
		{
			positions[i].X = (float) (random.NextDouble() * GraphicsDeviceManager.DefaultBackBufferWidth)
				- boxes[0].Width / 2;
			positions[i].Y = (float) (random.NextDouble() * GraphicsDeviceManager.DefaultBackBufferHeight)
				- boxes[0].Height / 2;
			depths[i] = (float) random.NextDouble();
			colors[i].R = (byte) (random.NextDouble() * 255);
			colors[i].G = (byte) (random.NextDouble() * 255);
			colors[i].B = (byte) (random.NextDouble() * 255);
			colors[i].A = (byte) (random.NextDouble() * 255);
			if (mode == SpriteSortMode.Texture)
			{
				boxRefs[i] = boxes[(int) (random.NextDouble() * TEXTURECOUNT)];
			}
			else
			{
				boxRefs[i] = boxes[0];
			}
		}

		previousKeyboardState = currentKeyboardState;
	}

	private void WriteStats()
	{
		if (stats.Count > 1)
		{
			Console.WriteLine();
		}

		Console.WriteLine("Technique: {0}", impl);
		Console.WriteLine("{0,15}{1,8}{2,8}{3,8}{4,8}", "Type", "Average", "Min", "Max", "Median");
		foreach (KeyValuePair<SpriteSortMode, Stat> series in stats)
		{
			Stat seriesValue = series.Value;
			Console.WriteLine("{0,15}{1,8:0}{2,8:0}{3,8:0}{4,8:0}", series.Key, seriesValue.Average, seriesValue.Min,
				seriesValue.Max, seriesValue.Median);
		}
	}

	protected override void LoadContent()
	{
		Color[] color = new Color[TEXTURESIZE * TEXTURESIZE];
		for (int i = 0; i < color.Length; i += 1)
		{
			color[i] = TEXTURECOLOR;
		}

		boxes = new Texture2D[TEXTURECOUNT];
		for (int i = 0; i < TEXTURECOUNT; i += 1)
		{
			boxes[i] = new Texture2D(GraphicsDevice, 128, 128);
			boxes[i].SetData(color);
		}

		batch = new SpriteBatch(GraphicsDevice);
	}

	protected override void UnloadContent()
	{
		batch.Dispose();
		batch = null;
		for (int i = 0; i < TEXTURECOUNT; i += 1)
		{
			boxes[i].Dispose();
			boxes[i] = null;
		}

		boxes = null;
	}

	public static void Main(string[] args)
	{
		using (SpriteBatchTestGame game = new SpriteBatchTestGame())
		{
			game.Run();
		}
	}
}
