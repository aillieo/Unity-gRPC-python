import logging
import asyncio

import grpc
import threading
import time

_cleanup_coroutines = []

import protos.multiplayer_pb2 as multiplayer_pb2
import protos.multiplayer_pb2_grpc as multiplayer_pb2_grpc

class MultiplayerServer(multiplayer_pb2_grpc.MultiplayerServicer):

    def __init__(self):

        self.sid = 0
        self.users = {}
        self.lock = threading.Lock()
        self.send_event = asyncio.Event()

    async def Join(self, request, context):
        with self.lock:
            if not context in self.users.values():
                self.sid = self.sid + 1
                uid = self.sid
                self.users[uid] = context
            return multiplayer_pb2.JoinResponse(uid=uid, pos=request.pos)

    async def Move(self, request, context):
        with self.lock:
            if not context in self.users.values():
                for uid, ctx in self.users.items():
                    self.send_event.set()
                return multiplayer_pb2.MoveResponse(pos=request.pos)
            else:
                for uid, ctx in self.users.items():
                    self.send_event.set()
                return multiplayer_pb2.MoveResponse(pos=request.pos)

    async def SyncPos(self, request_iterator, context):
        while not self.users:
            await asyncio.sleep(0.1)

        while self.users:
            await self.send_event.wait()
            self.send_event.clear()
            with self.lock:
                if self.users:
                    for uid, ctx in self.users.items():
                        logging.info("push")
                        yield multiplayer_pb2.PushSyncPos(uid=1, pos=multiplayer_pb2.Position(x=1, y=1))

async def serve() -> None:
    server = grpc.aio.server()
    multiplayer_pb2_grpc.add_MultiplayerServicer_to_server(MultiplayerServer(), server)
    listen_addr = '[::]:50051'
    server.add_insecure_port(listen_addr)
    logging.info("Starting server on %s" % listen_addr)
    await server.start()

    async def server_graceful_shutdown():
        logging.info("graceful_shutdown")
        await server.stop(5)

    _cleanup_coroutines.append(server_graceful_shutdown())

    await server.wait_for_termination()


def cleanup_coroutines() -> None:
    return _cleanup_coroutines
