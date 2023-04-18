import CalculatorServer
import MultiplayerServer
import FigureRecognitionServer
import asyncio
import logging


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO)
    loop = asyncio.get_event_loop()

    # server = CalculatorServer
    # server = MultiplayerServer
    server = FigureRecognitionServer

    try:
        loop.run_until_complete(server.serve())
    finally:
        loop.run_until_complete(*server.cleanup_coroutines())
        loop.close()
