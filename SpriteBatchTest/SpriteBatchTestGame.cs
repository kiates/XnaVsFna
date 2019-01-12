/* SpriteBatch Stress Test
 * Written by Ethan "flibitijibibo" Lee
 * http://www.flibitijibibo.com/
 *
 * Released under public domain.
 * No warranty implied; use at your own risk.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

class SpriteBatchTestGame : Game
{
  private const int SPRITECOUNT = 2048;
  private const int TEXTURECOUNT = 2;

  private const int TEXTURESIZE = 128;
  private static readonly Color TEXTURECOLOR = Color.White;

  private SpriteSortMode mode = SpriteSortMode.BackToFront;

  private Stopwatch timer;
  private Random random;
  private Vector2[] positions;
  private float[] depths;
  private Color[] colors;
  private Texture2D[] boxRefs;
  private Texture2D[] boxes;
  private SpriteBatch batch;

  public SpriteBatchTestGame() : base()
  {
    new GraphicsDeviceManager(this);
    timer = new Stopwatch();
    random = new Random();
    positions = new Vector2[SPRITECOUNT];
    depths = new float[SPRITECOUNT];
    colors = new Color[SPRITECOUNT];
    boxRefs = new Texture2D[SPRITECOUNT];
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
    //Console.WriteLine("Batch took {0} ticks ({1} ms with {2}.", timer.ElapsedTicks, timer.ElapsedMilliseconds, mode);
    stats.TryGetValue(mode, out List<double> stat2);
    if (stat2 == null)
    {
      stat2 = new List<double>(100);
      stats[mode] = stat2;
    }
    stat2.Add(timer.ElapsedTicks);
    timer.Reset();
  }

  private readonly Dictionary<SpriteSortMode, List<double>> stats = new Dictionary<SpriteSortMode, List<double>>();

  private KeyboardState currentKeyboardState;
  private KeyboardState previousKeyboardState;

  private bool IsKeyPressed(Keys key)
  {
    return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
  }

  protected override void Update(GameTime gameTime)
  {
    currentKeyboardState = Keyboard.GetState();

    if (IsKeyPressed(Keys.D))
    {
      foreach (KeyValuePair<SpriteSortMode, List<double>> series in stats)
      {
        double averageTicks = series.Value.Average();

        Console.WriteLine("{0}: {1} ticks on average", series.Key, averageTicks);
        //Console.WriteLine("{0}: {1} ticks on average", series.Key, series.Value.StdDev());
      }

      Console.WriteLine();
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
      positions[i].X = (float)(random.NextDouble() * GraphicsDeviceManager.DefaultBackBufferWidth) - (boxes[0].Width / 2);
      positions[i].Y = (float)(random.NextDouble() * GraphicsDeviceManager.DefaultBackBufferHeight) - (boxes[0].Height / 2);
      depths[i] = (float) random.NextDouble();
      colors[i].R = (byte)(random.NextDouble() * 255);
      colors[i].G = (byte)(random.NextDouble() * 255);
      colors[i].B = (byte)(random.NextDouble() * 255);
      colors[i].A = (byte)(random.NextDouble() * 255);
      boxRefs[i] = boxes[(int)(random.NextDouble() * TEXTURECOUNT)];
    }

    previousKeyboardState = currentKeyboardState;
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