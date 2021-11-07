import server
import asyncio
import logging

import grpc
if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO)
    loop = asyncio.get_event_loop()
    try:
        loop.run_until_complete(server.serve())
    finally:
        loop.run_until_complete(*_cleanup_coroutines)
        loop.close()
