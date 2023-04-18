import logging

import grpc
import protos.calculator_pb2 as calculator_pb2
import protos.calculator_pb2_grpc as calculator_pb2_grpc

_cleanup_coroutines = []


class CalculatorServer(calculator_pb2_grpc.CalculationServicer):

    def __init__(self):

        self.op_dict = {
            calculator_pb2.C2SRequest.Add: lambda lhs, rhs: lhs + rhs,
            calculator_pb2.C2SRequest.Subtract: lambda lhs, rhs: lhs - rhs,
            calculator_pb2.C2SRequest.Multiply: lambda lhs, rhs: lhs * rhs,
            calculator_pb2.C2SRequest.Divide: lambda lhs, rhs: lhs // rhs
        }

    async def Calculate(
            self, request: calculator_pb2.C2SRequest,
            context: grpc.aio.ServicerContext) -> calculator_pb2.S2CReply:
        logging.info('Received from client : %s' % str(request))
        result = self.op_dict[request.op](request.lhs, request.rhs)
        return calculator_pb2.S2CReply(message=str(request), result=result)


async def serve() -> None:
    server = grpc.aio.server()
    calculator_pb2_grpc.add_CalculationServicer_to_server(CalculatorServer(), server)
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
