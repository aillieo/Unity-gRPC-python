using Grpc.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sample
{
    public class SampleFigureRecognition : MonoBehaviour
    {
        public RawImage rawImage;
        public Button button;
        public Text resultText;

        public int brushSize = 10;
        public Color brushColor = Color.black;
        public Vector2Int imageSize = new Vector2Int(64, 64);

        private Texture2D texture;
        private Canvas canvas;
        private Vector2 lastPos;

        private void Start()
        {
            texture = new Texture2D(imageSize.x, imageSize.y, TextureFormat.R8, false, false);
            rawImage.texture = texture;

            canvas = this.GetComponent<Canvas>();
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 normalizedPos = MousePointToImageLocal();
                if (lastPos == Vector2.zero)
                {
                    lastPos = normalizedPos;
                }

                DrawLine(lastPos, normalizedPos, brushSize, brushColor);
                lastPos = normalizedPos;
            }
            else
            {
                lastPos = Vector2.zero;
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
            float distance = Vector2.Distance(localCoord0, localCoord1);

            int steps = Mathf.RoundToInt(distance * texture.width / radius);

            if (steps <= 0)
            {
                DrawPoint(localCoord0, radius, color);
                return;
            }

            float stepSize = 1.0f / steps;

            for (int i = 0; i <= steps; i++)
            {
                float t = i * stepSize;
                Vector2 point = Vector2.Lerp(localCoord0, localCoord1, t);
                DrawPoint(point, radius, color);
            }
        }

        private void DrawPoint(Vector2 localCoord, int radius, Color color)
        {
            int x = (int)(localCoord.x * texture.width);
            int y = (int)(localCoord.y * texture.height);

            for (int i = x - radius; i <= x + radius; i++)
            {
                if (i < 0 || i >= texture.width)
                {
                    continue;
                }

                for (int j = y - radius; j <= y + radius; j++)
                {
                    if (j < 0 || j >= texture.height)
                    {
                        continue;
                    }

                    float distance = Vector2.SqrMagnitude(new Vector2(i - x, j - y));
                    if (distance <= radius)
                    {
                        texture.SetPixel(i, j, color);
                    }
                }
            }

            texture.Apply();
        }
    }
}
