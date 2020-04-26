Prerequisites: [.NET SDK 3.1](https://dot.net/)
To run a dev server: `dotnet watch -p Cardgame run`

Cardgame is split into client and server pieces; the top-level app connects them together using blazor server, implementing the hosting interfaces from Cardgame.Client using shared memory. In theory the client could be WASM instead. Component layering:
```
                  ♣----------♣
                  | Cardgame |
                  ♣-----+----♣            
                 /      |     \
                v       |      v
               /        |       \
     ♦--------♦    ♥----+---♥    ♠-------♠
     | Client |    | Server |    | Cards |
     ♦---+----♦    ♥---+----♥    ♠-------♠
          \           /         / 
           v         v         v   
            \       /         /      
             ♠-----♠   ♠-----♠
             | All +->-+ API |
             ♠--+--♠   ♠-----♠
                |
                v           ♣ composition
                |           ♦ frontend
            ♠---+---♠       ♥ backend
            | Model |       ♠ common
            ♠-------♠
```

The point of the All/API split is to prevent card implementations from depending on how the engine actually works; Model contains the types which the server and client use to describe gamestate.