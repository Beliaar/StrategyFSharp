module Strategy.Systems.HexGrid

open Microsoft.Xna.Framework
open Strategy.Components
open Strategy.Components.Hexagon

let GROUND_BIT = 0
let UNIT_BIT = 1

let CreateGrid radius =

    let CreateCube (q, r) = Hexagon.NewAxial q r

    let InRadius (hexagon: Hexagon) =
        (hexagon.DistanceTo Hexagon.Zero) < radius

    let hexagons =
        let array = [| -radius .. radius + 1 |]

        array
        |> Array.collect (fun r -> array |> Array.map (fun s -> (r, s)))
        |> Array.map CreateCube
        |> Array.filter InRadius

    hexagons

let Get2DPositionOfHexagon (hexagon: Hexagon) hexFieldSize =
    let x =
        hexFieldSize
        * (sqrt 3f * float32 hexagon.Q
           + sqrt 3f / 2f * float32 hexagon.R)

    let y =
        hexFieldSize * (3f / 2f * float32 hexagon.R)

    Vector2(x, y)

let GetNeighbours (hexagon: Hexagon) =
    [| hexagon.GetNeighbour(Direction.East),
       hexagon.GetNeighbour(Direction.NorthEast),
       hexagon.GetNeighbour(Direction.NorthWest),
       hexagon.GetNeighbour(Direction.West),
       hexagon.GetNeighbour(Direction.SouthWest),
       hexagon.GetNeighbour(Direction.SouthEast) |]


let PolygonWidth hexfieldSize = sqrt 3f * hexfieldSize

let PolygonHeight hexfieldSize = 2f * hexfieldSize

let Half value = value / 2f
let Quarter value = value / 4f

let PolygonPoints hexfieldSize =
    let width = PolygonWidth hexfieldSize
    let height = PolygonHeight hexfieldSize
    let halfHeight = Half height
    let quarterHeight = Quarter height
    let halfWidth = Half width

    [| Vector2(-halfWidth, -quarterHeight)
       Vector2(0f, -halfHeight)
       Vector2(halfWidth, -quarterHeight)
       Vector2(halfWidth, quarterHeight)
       Vector2(0f, halfHeight)
       Vector2(-halfWidth, quarterHeight)
       Vector2(-halfWidth, -quarterHeight) |]

let PolygonTriangles hexfieldSize =
    let points = PolygonPoints hexfieldSize
    let centre = Vector2.Zero

    let closeCircuit points =
        points |> Array.take 1 |> Array.append points

    let makeTriangles points =
        points
        |> Array.pairwise
        |> Array.map (fun (p1, p2) -> [| centre; p1; p2 |])
        |> Array.fold Array.append [||]

    points |> closeCircuit |> makeTriangles
