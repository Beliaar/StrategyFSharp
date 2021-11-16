namespace Strategy

module Program =

    open System
    open Microsoft.Xna.Framework

    [<EntryPoint>]
    let main argv =
        use game = new StrategyGame()
        game.Run()
        0 // return an integer exit code
