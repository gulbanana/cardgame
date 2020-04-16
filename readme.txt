in theory, this app could be split into client and server pieces with a WASM frontend. the components are strictly layered:

                     +---------+
                     |  Main   +-
                     +---------+ \            
                   /              \
                  v                \
                 /                  v
     +---------+     +---------+     \
     |  Pages  |     | Protocol+-<    \ 
     +----+----+     +----+----+  \    |
          |        /      |        \ +-+-------+
          v       ^       v         -+ Hosting |
          |      /        |          +----+----+
     +----+----+     +----+----+          |
     |  Views  |     |  Shared |          v
     +----+----+     |----+----+          |
          |      \        |          +----+----+
          v       v       ^         -+ Backend |
          |        \      |        / +---------+           
     +----+----+     +----+----+  /
     | Widgets |     |   All   +-<
     +---------+     +----+----+
                          |      
                          v      
                          |         
                     +----+----+     +---------+
                     |   API   +--<--+  Cards  |
                     +---------+     +---------+