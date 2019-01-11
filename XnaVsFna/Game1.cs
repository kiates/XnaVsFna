using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace WindowsGame1
{
  /// <summary>
  /// This is the main type for your game
  /// </summary>
  public class Game1 : Microsoft.Xna.Framework.Game
  {
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    private Texture2D sprite;

    private Random random = new Random(0);

    public Game1()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      IsFixedTimeStep = false;
      graphics.SynchronizeWithVerticalRetrace = false;
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
      // TODO: Add your initialization logic here

      base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      // Create a new SpriteBatch, which can be used to draw textures.
      spriteBatch = new SpriteBatch(GraphicsDevice);

      sprite = Content.Load<Texture2D>("600px-Uvrefmap_blackwhite");
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// all content.
    /// </summary>
    protected override void UnloadContent()
    {
      // TODO: Unload any non ContentManager content here
    }

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
      // Allows the game to exit
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
        this.Exit();

      // TODO: Add your update logic here

      base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);

      // TODO: Add your drawing code here

      Vector2 viewportDimensions = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

      spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
      {
        float scale = .125f;
        Vector2 spriteDimensions = new Vector2(sprite.Width, sprite.Height)*scale;
        Vector2 spriteOrigin = new Vector2(sprite.Width, sprite.Height) / 2;
        const int numSprites = 1000;
        for (int i = 0; i < numSprites; ++i)
        {
          //Vector2 position = new Vector2(random.Next(1280), random.Next(720));
          float amount = (float)i/(numSprites-1);
          Vector2 position = spriteOrigin*scale 
            + new Vector2(
              MathHelper.Lerp(0, viewportDimensions.X-spriteDimensions.X, amount), 
              MathHelper.Lerp(0, viewportDimensions.Y-spriteDimensions.Y, amount));
          //float depth = (float)random.NextDouble();
          float depth = MathHelper.Lerp(0, 1, amount);
          spriteBatch.Draw(sprite, position, null, Color.White, 0, spriteOrigin, scale, SpriteEffects.None, depth);
        }
      }
      spriteBatch.End();

      base.Draw(gameTime);
    }
  }
}
