module Strategy.Components.Field

open Strategy.Components.Hexagon

type Field =
    struct
        val Location: Hexagon
        val Movable: bool
        val Attackable: bool

        new(hexagon, movable, attackable) =
            { Location = hexagon
              Movable = movable
              Attackable = attackable }

        static member FromHexagon hexagon = Field(hexagon, false, false)
    end
