namespace Strategy

open System
open System.Drawing
open System.Numerics
open System.Numerics
open FSharp.Linq.NullableOperators
open Garnet.Composition
open Garnet.Composition.Join
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open MonoGame.Extended
open Strategy.Components
open Strategy.Components.Field
open Strategy.Systems.HexGrid

[<Struct>] type Draw = {DrawTime: GameTime}
[<Struct>] type Update = {UpdateTime: GameTime}

[<Struct>] type Position = {X : float32; Y : float32}
[<Struct>] type HexGrid = {FieldSize: int32; Radius: int32}

type StrategyGame () as this =
    inherit Game()
 
    let world = Container()
    let mutable updateLOS = true
    let mutable leftButtonHeld = false
    let mutable update = Unchecked.defaultof<_>
    let mutable draw = Unchecked.defaultof<_>
    let graphics = new GraphicsDeviceManager(this)
    let mutable losPoint = Vector2.Zero

    do
        this.Content.RootDirectory <- "Content"
        this.IsMouseVisible <- true

    override this.Initialize() =
        // TODO: Add your initialization logic here       
                    
        let distanceFromCenter vector = Vector2.Distance(losPoint, vector) 
        let intersects_distance (position : Vector2) radius (ray: Ray) distance =
            let intersects = ray.Intersects(BoundingSphere(Vector3(position.X, position.Y, 0f), radius))            
            
            if  intersects ?<= distance then
                true
            else
                false                    
        let intersects (position: Vector2) =
            let direction = position - losPoint
            direction.Normalize()
            let distance = distanceFromCenter position
            let ray = Ray(Vector3(losPoint.X, losPoint.Y, 0f), Vector3(direction.X, direction.Y, 0f))
            let filter (otherPosition, radius) = (not <| position.Equals(otherPosition))
                                                 &&
                                                 intersects_distance otherPosition radius ray distance
            false
                    
            
        update <- world.On<Update> <| fun time ->
            
            let mouseState = world.LoadResource<MouseState> "MouseState"
            world.AddResource ("MousePos", Vector2(float32 mouseState.X, float32 mouseState.Y))
            
            let leftPressed = 
                match mouseState.LeftButton with
                | ButtonState.Pressed -> true
                | ButtonState.Released -> false
                | _ -> false
            world.AddResource ("LeftButtonHeld", leftPressed)
            
            
            let recreateGrid = world.LoadResource<bool> "RecreateGrid"           
          

            if recreateGrid then            
                for field in world.Query<Eid, Field>() do
                    world.Destroy(field.Value1)
                let radius = world.LoadResource<Int32> "Radius"
                let grid = CreateGrid radius
                grid
                |> Array.map Field.FromHexagon
                |> Array.iter (fun hex -> world.Create().With hex |> ignore)
                world.AddResource ("RecreateGrid", false)
            
                
            if updateLOS then
                updateLOS <- false
                for field in world.Query<Eid, Position, Field>() do
                    let position = Vector2(field.Value2.X, field.Value2.Y)
                    let canSee = not <| intersects position
                    let entity = world.Get field.Current.Value1
                    let field = field.Value3
                    let newField = Field(field.Location, false, canSee)
                    entity.Set<Field> newField
        draw <- world.On<Draw> <| fun time ->
            let spriteBatch = world.LoadResource<SpriteBatch> "SpriteBatch"
            let hexfieldSize = world.LoadResource<float32> "FieldSize"
            let centre = world.LoadResource<Vector2> "Centre"
            let polygon = CalculatePolygon hexfieldSize
            spriteBatch.Begin()
            let query = world.Query<Field>()
            for field in query do
                let position = Get2DPositionOfHexagon field.Value.Location hexfieldSize + centre
                spriteBatch.DrawPolygon(position, polygon, Color.White)
            spriteBatch.End()
                
        
        base.Initialize() // Load content has been called after
        losPoint <- Vector2(float32 graphics.PreferredBackBufferWidth / 2f, float32 graphics.PreferredBackBufferHeight / 2f)

        
//        let random = Random()

//        let createObject _ =
//            world.Create()
               
            

    override this.LoadContent() =
        world.AddResource ("FieldSize", 40f)
        world.AddResource ("Radius", 10)
        world.AddResource ("RecreateGrid", true)
        world.AddResource ("SpriteBatch", new SpriteBatch(this.GraphicsDevice))
        world.AddResource ("MousePos", Vector2.Zero)
        world.AddResource ("LeftButtonHeld", false)
        world.AddResource ("MouseState", Mouse.GetState this.Window)
        world.AddResource ("Centre", Vector2(float32 graphics.PreferredBackBufferWidth / 2f, float32 graphics.PreferredBackBufferHeight / 2f))
        // TODO: use this.Content to load your game content here
 
    override this.Update (gameTime) =
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        then this.Exit()
        world.AddResource ("MouseState", Mouse.GetState this.Window)
        world.Run <| {UpdateTime = gameTime}    
        base.Update(gameTime)
 
    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue
        
        world.Run <| {DrawTime = gameTime}
        
                
        // TODO: Add your drawing code here

        base.Draw(gameTime)

