import logging

import grpc
import protos.figurerecognition_pb2 as figurerecognition_pb2
import protos.figurerecognition_pb2_grpc as figurerecognition_pb2_grpc

import torch
import torchvision.transforms as transforms
from PIL import Image

_cleanup_coroutines = []


class FigureRecognitionServer(figurerecognition_pb2_grpc.FigureRecognitionServicer):

    def __init__(self):
        # load model
        # self.model = torch.load("xxx.pt")
        self.model = torch.hub.load('pytorch/vision:v0.6.0', 'mnist', pretrained=True)
        pass

    async def Recognize(
            self, request: figurerecognition_pb2.RecognitionRequest,
            context: grpc.aio.ServicerContext) -> figurerecognition_pb2.RecognitionResponse:
        logging.info('Received from client : %s' % str(request))

        base64 = request.image
        # decode

        # resize
    
        # recognize

        # define the transformation
        transform = transforms.Compose([
            transforms.Resize((28, 28)),
            transforms.Grayscale(),
            transforms.ToTensor(),
        ])

        # apply the transformation
        img = transform(img)

        # reshape the tensor to match the expected input shape of the model
        img = torch.reshape(img, (1, 1, 28, 28))

        # Pass the image through the model to get the predicted class
        with torch.no_grad():
            output = self.model(img)
            _, predicted = torch.max(output.data, 1)

        # print the predicted class
        result = predicted.item()
        print("Predicted class:", result)

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
