In theory, this app could be split into client and server pieces with a WASM frontend. The components are strictly layered:

                     ♣---------♣
                     |  Main   |
                     ♣---------♣            
                   /             \
                  v               v
                 /                 \
     ♦---------♦     ♠---------♠     ♥---------♥
     |  Pages  |     |  Client +--<--+ Hosting |
     ♦----+----♦     ♠----+----♠     ♥----+----♥
          |        /      |               |
          v       ^       v               v
          |      /        |               |
     ♦----+----♦     ♠----+----♠     ♥----+----♥
     |  Views  |     | Shared  |     | Server  |
     ♦----+----♦     ♠----+----♠     ♥---------♥
          |      \        |        /
          v       v       ^       v
          |        \      |      /
     ♦----+----♦     ♠----+----♠ 
     | Widgets |     |   All   |
     ♦---------♦     ♠----+----♠
                          |      
                          v      
                          |         
                     ♠----+----♠
                     |   API   |
                     ♠----+----♠
                          |
                          ^
♣ composition             |
♦ frontend           ♠----+----♠
♥ backend            |  Cards  |
♠ common             ♠---------♠

For a client-server version, replace the Hosting namespace which uses shared-memory with alternate implementations of the Client namespace that communicate with Server over an API.