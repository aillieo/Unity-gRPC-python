using Grpc.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace Sample
{
    public class SampleCalculator : MonoBehaviour
    {
        public InputField lhInput;
        public InputField rhInput;
        public Dropdown opDrop;
        public Text resultText;

        public async void SendMessage()
        {
            try
            {
                Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

                Sample.Calculation.CalculationClient client = new Sample.Calculation.CalculationClient(channel);

                var req = new Sample.C2SRequest();
                int.TryParse(lhInput.text, out int l);
                req.Lhs = l;

                int.TryParse(rhInput.text, out int r);
                req.Rhs = r;

                int op = opDrop.value;
                req.Op = (Sample.C2SRequest.Types.Operation)op;

                Debug.Log("req=" + req);

                var reply = await client.CalculateAsync(req);

                //Response.text = reply.Message;
                resultText.text = reply.Result.ToString();

                Debug.Log("rep=" + reply);

                channel.ShutdownAsync().Wait();
            }
            catch (Exception e)
            {
                resultText.text = "exception: " + e.Message;
                Debug.LogError(e.Message);
            }
        }
    }
}
