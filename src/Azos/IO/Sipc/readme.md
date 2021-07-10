# SIPC - Simple IPC


Simple IPC aka **`sipc`** provides bidirectional inter-process communication mechanism
based on a TCP socket. The communication is typically established on a local-loop interface on the same host.

Sipc is usually used by governors (application process management architecture) to communicate between 
application process instances which are activated by master governor process on the same host.
Some process governance models may also activate subordinate processes in containers (such as Docker),
in which case TCP connection is established from a process in container (which is being activated)
back into the central parent governor process (that activates the child).


Sipc protocol is purposely simple and primitive, as it was designed for app management.

1. There is a sipc server listening on `localhost:x` where x is derived from port range which you provide at start
2. The server tries to bind a listener and finally binds it to a listener port in the selected port range
3. The server communicates the assigned listener port to subordinate processes, e.g. via command line argument passing at launch
4. Subordinate applications connect to server on the specified port
5. The communication is bi-directionals socket
6. The protocol exchanges string commands
7. The handshake: client sends unique "connection name" to server, which server can use to link-up `Connection` instance to some business object (e.g. a "managed application")
8. Override SipcClient/Server classes `DoHandleCommand(string command)` to handle commands
9. Do not block for long as `DoHandle()` handlers are called by a single central thread
10. Do not leak errors - handle all errors gracefully via logging/tracing


 


