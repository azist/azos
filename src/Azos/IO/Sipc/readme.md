# SIPC - Simple IPC


Simple IPC aka `sipc` provides on-node bidirectional inter-process communication mechanism
using a local loop socket.

Sipc is used for APM (application process management protocol) to communicate between 
application process instances on the same host.

Sipc protocol is primitive.
1. There is a sipc server listening on `localhost:x` where x is derived from port range

