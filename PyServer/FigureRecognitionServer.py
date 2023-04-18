import logging

import grpc
import protos.figurerecognition_pb2 as figurerecognition_pb2
import protos.figurerecognition_pb2_grpc as figurerecognition_pb2_grpc

import base64
from io import BytesIO
from PIL import Image
import torchvision.transforms as transforms
import torch


_cleanup_coroutines = []


class FigureRecognitionServer(figurerecognition_pb2_grpc.FigureRecognitionServicer):

    def __init__(self):
        # load model
        import SimpleMnistCNN
        self.model = SimpleMnistCNN.SimpleMnistCNN()
        self.model.load_state_dict(torch.load("./model/simple_mnist_cnn.pth"))
        self.model.eval()

    async def Recognize(
            self, request: figurerecognition_pb2.RecognitionRequest,
            context: grpc.aio.ServicerContext) -> figurerecognition_pb2.RecognitionResponse:
        logging.info('Received from client : %s' % str(request))

        # string to bytes
        bytes = base64.b64decode(request.image)

        # bytes to PIL.image
        fp = BytesIO(bytes)
        img = Image.open(fp)

        # PIL.image to tensor
        transform = transforms.Compose([
            transforms.Resize((28, 28)),
            transforms.Grayscale(),
            transforms.ToTensor(),
        ])
        img = transform(img)

        # tensor 28x28 to tensor 1x784
        img = torch.reshape(img, (1, 784))

        # predict
        with torch.no_grad():
            output = self.model(img)
            logging.info(output.data)
            _, predicted = torch.max(output.data, 1)

        result = predicted.item()
        logging.info(f"result:{result}")

        return figurerecognition_pb2.RecognitionResponse(number=result)

async def serve() -> None:
    server = grpc.aio.server()
    figurerecognition_pb2_grpc.add_FigureRecognitionServicer_to_server(FigureRecognitionServer(), server)
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
