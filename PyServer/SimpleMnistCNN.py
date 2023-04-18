import torch.nn.functional as F
from torch import nn
from os import path
import pickle
import gzip
import torch
from torch import optim

bs = 128
lr = 0.1
epochs = 50

class SimpleMnistCNN(nn.Module):
    def __init__(self):
        super().__init__()
        self.conv1 = nn.Conv2d(1, 16, kernel_size=3, stride=2, padding=1)
        self.conv2 = nn.Conv2d(16, 16, kernel_size=3, stride=2, padding=1)
        self.conv3 = nn.Conv2d(16, 10, kernel_size=3, stride=2, padding=1)

    def forward(self, xb):
        xb = xb.view(-1, 1, 28, 28)
        xb = F.relu(self.conv1(xb))
        xb = F.relu(self.conv2(xb))
        xb = F.relu(self.conv3(xb))
        xb = F.avg_pool2d(xb, 4)
        return xb.view(-1, xb.size(1))

if __name__ == "__main__":

    # https://github.com/pytorch/tutorials/raw/main/_static/mnist.pkl.gz
    data_path = "./data/mnist/mnist.pkl.gz"
    if not path.isfile(data_path):
        raise FileNotFoundError(data_path)


    with gzip.open(data_path, "rb") as f:
            ((x_train, y_train), (x_valid, y_valid), _) = pickle.load(f, encoding="latin-1")


    x_train, y_train, x_valid, y_valid = map(
        torch.tensor, (x_train, y_train, x_valid, y_valid)
    )
    n, c = x_train.shape


    model = SimpleMnistCNN()
    model_path = "./model/simple_mnist_nn.pth"
    if path.isfile(model_path):
        model.load_state_dict(torch.load(model_path))
        model.eval()


    loss_func = F.cross_entropy
    opt = optim.SGD(model.parameters(), lr=lr, momentum=0.9)


    for epoch in range(epochs):
        for i in range((n - 1) // bs + 1):
            start_i = i * bs
            end_i = start_i + bs
            xb = x_train[start_i:end_i]
            yb = y_train[start_i:end_i]
            pred = model(xb)
            loss = loss_func(pred, yb)

            loss.backward()
            opt.step()
            opt.zero_grad()
        print(f"{epoch}  {loss_func(model(xb), yb)}")

    torch.save(model.state_dict(), "./model/simple_mnist_nn.pth")
