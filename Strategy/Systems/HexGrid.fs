module Strategy.Systems.HexGrid

open System
open Microsoft.Xna.Framework
open MonoGame.Extended.Shapes
open MonoGame.Extended.Shapes
open Strategy.Components
open Strategy.Components.Hexagon

let GROUND_BIT = 0
let UNIT_BIT = 1

let CreateGrid radius =

    let CreateCube (q, r) =        
        Hexagon.NewAxial q r

    let InRadius (hexagon: Hexagon) =
        (hexagon.DistanceTo Hexagon.Zero) < radius

    let hexagons = 
        let array = [| -radius .. radius + 1 |]
        array
        |> Array.collect (fun r ->
            array |> Array.map (fun s -> (r, s)))
        |> Array.map CreateCube
        |> Array.filter InRadius        
        
    hexagons

let Get2DPositionOfHexagon (hexagon : Hexagon) hexFieldSize =
    let x = hexFieldSize * (sqrt 3f * float32 hexagon.Q + sqrt 3f / 2f * float32 hexagon.R)
    let y = hexFieldSize * (3f / 2f * float32 hexagon.R);
    Vector2(x, y)

let GetNeighbours (hexagon: Hexagon) =    
    [|
        hexagon.GetNeighbour(Direction.East),
        hexagon.GetNeighbour(Direction.NorthEast),
        hexagon.GetNeighbour(Direction.NorthWest),
        hexagon.GetNeighbour(Direction.West),
        hexagon.GetNeighbour(Direction.SouthWest),
        hexagon.GetNeighbour(Direction.SouthEast)
    |]


let CalculatePolygon hexfield_size =
    let width = sqrt 3f * hexfield_size;
    let height = 2f * hexfield_size;
    let half_height = height / 2f;
    let quarter_height = height / 4f;

    let half_width = width / 2f;

    
    let points = [|
        Vector2(-half_width, -quarter_height);
        Vector2(0f, -half_height);
        Vector2(half_width, -quarter_height);
        Vector2(half_width, quarter_height);
        Vector2(0f, half_height);
        Vector2(-half_width, quarter_height);
        Vector2(-half_width, -quarter_height);
    |]
    Polygon (points)