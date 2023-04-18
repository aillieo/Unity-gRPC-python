using Grpc.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class SampleFigureRecognition : MonoBehaviour
    {
        public RawImage rawImage;
        public Button button;
        public Text resultText;

        private readonly int brushSize = 1;
        private readonly Color brushColor = Color.white;
        private readonly Vector2Int imageSize = new Vector2Int(56, 56);

        private Texture2D texture;
        private Canvas canvas;
        private Vector2? lastPos;

        private void Start()
        {
            texture = new Texture2D(imageSize.x, imageSize.y, TextureFormat.RGB24, false, true);
            texture.SetPixels(Enumerable.Repeat(Color.black, imageSize.x * imageSize.y).ToArray());
            texture.Apply();

            rawImage.texture = texture;

            canvas = this.GetComponent<Canvas>();
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 normalizedPos = MousePointToImageLocal();

                if (!lastPos.HasValue)
                {
                    lastPos = normalizedPos;
                }

                DrawLine(lastPos.Value, normalizedPos, brushSize, brushColor);
                lastPos = normalizedPos;
            }
            else
            {
                lastPos = null;
            }
        }

        public async void RequestRecognition()
        {
            try
            {
                Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

                FigureRecognition.FigureRecognitionClient client = new FigureRecognition.FigureRecognitionClient(channel);

                byte[] bytes = ImageConversion.EncodeToPNG(this.texture);
                string base64 = Convert.ToBase64String(bytes);

                var req = new RecognitionRequest()
                {
                    Image = base64,
                };

                Debug.Log("req=" + req);

                var reply = await client.RecognizeAsync(req);

                Debug.Log("rep=" + reply);

                resultText.text = reply.Number.ToString();

                channel.ShutdownAsync().Wait();
            }
            catch (Exception e)
            {
                resultText.text = "exception: " + e.Message;
                Debug.LogError(e.Message);
            }
        }

        private Vector2 MousePointToImageLocal()
        {
            Vector2 mouseScreenPosition = Input.mousePosition;
            RectTransform rawImageRT = rawImage.rectTransform;
            RectTransform canvasRT = canvas.transform as RectTransform;

            Vector2 positionInCanvas;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, mouseScreenPosition, canvas.worldCamera, out positionInCanvas);
            Vector2 localPosition = rawImageRT.InverseTransformPoint(canvasRT.TransformPoint(positionInCanvas));

            Vector2 size = rawImageRT.rect.size;
            Vector2 normalized = (localPosition + size * 0.5f) / size;

            return normalized;
        }

        private void DrawLine(Vector2 localCoord0, Vector2 localCoord1, int radius, Color color)
        {
            Vector2 point0 = new Vector2(localCoord0.x * texture.width, localCoord0.y * texture.height);
            Vector2 point1 = new Vector2(localCoord1.x * texture.width, localCoord1.y * texture.height);

            float distance = Vector2.Distance(point0, point1);

            int steps = Mathf.RoundToInt(distance / radius);

            if (steps <= 0)
            {
                DrawPoint(point0, radius, color);
                return;
            }

            float stepSize = 1.0f / steps;

            for (int i = 0; i <= steps; i++)
            {
                float t = i * stepSize;
                Vector2 point = Vector2.Lerp(point0, point1, t);
                DrawPoint(point, radius, color);
            }
        }

        private void DrawPoint(Vector2 imageCoord, int radius, Color color)
        {
            float width = texture.width;
            float height = texture.height;
            float sqRadius = radius * radius;

            int x = (int)imageCoord.x;
            int y = (int)imageCoord.y;

            for (int i = x - radius; i <= x + radius; i++)
            {
                if (i < 0 || i >= width)
                {
                    continue;
                }

                for (int j = y - radius; j <= y + radius; j++)
                {
                    if (j < 0 || j >= height)
                    {
                        continue;
                    }

                    float sqDistance = Vector2.SqrMagnitude(new Vector2(i - x, j - y));
                    if (sqDistance <= sqRadius)
                    {
                        texture.SetPixel(i, j, color);
                    }
                }
            }

            texture.Apply();
        }
    }
}
