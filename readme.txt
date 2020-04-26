Prerequisites: [.NET SDK 3.1](https://dot.net/)
To run a dev server: `dotnet watch -p Cardgame run`

Cardgame is split into three pieces: the game engine, a web-based UI and card implementations. Component layering:

```
      ♦-----♦      ♥--------♥    ♠-------♠
      | GUI |      | Engine |    | Cards |
      ♦--+--♦      ♥---+----♥    ♠-------♠
          \           /         / 
           v         v         v   
            \       /         /      
             ♣-----♣   ♠-----♠
             | All +->-+ API |
             ♣--+--♣   ♠-----♠
                |
                v           ♣ composition
                |           ♦ frontend
            ♠---+---♠       ♥ backend
            | Model |       ♠ common
            ♠-------♠
```

The point of the All/API split is to prevent card implementations from depending on how the engine actually works; Model contains the types which the server and client use to describe gamestate.

Currently the top-level app connects them together using Blazor Server, implementing the hosting interfaces from Cardgame.UI using shared memory:

```
                  ♣----------♣
                  | Cardgame |
                  ♣-----+----♣            
                 /      |     \
                v       v      v
               /        |       \
        ♦-----♦    ♥----+---♥    ♠-------♠
        | GUI |    | Engine |    | Cards |
        ♦-----♦    ♥--------♥    ♠-------♠
```

In theory the client could be WASM instead, with a structure like this:

```
♦--------♦   ♠----------♠   ♥--------♥
| Client |->-| Protocol |-<-| Server |
♦----+---♦   ♠----------♠   ♥---v----♥        
     |    \               /     |
     v     v             v      v
     |      \           /       |
  ♦--+--♦     ♠-------♠     ♥---+----♥
  | GUI |     | Cards |     | Engine |  
  ♦-----♦     ♠-------♠     ♥--------♥
```