using Grpc.Core;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Sample
{
    public class SampleMultiplayer : MonoBehaviour
    {
        private Channel channel;
        private Multiplayer.MultiplayerClient client;

        [SerializeField]
        private PlayerController playerPrefab;
        private PlayerController playerInstance;
        private CancellationTokenSource cts = new CancellationTokenSource();

        private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();

        private void Awake()
        {
            this.channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            this.client = new Multiplayer.MultiplayerClient(channel);
        }

        private void OnDestroy()
        {
            cts.Cancel();

            channel.ShutdownAsync().Wait();
            channel = null;
            client = null;
        }

        private async void Start()
        {
            await RequestJoin();

            await RegisterSyncPos();
        }

        private async Task RequestJoin()
        {
            Vector3 position = this.transform.position;
            var req = new JoinRequest();
            req.Pos = new Position() { X = position.x, Y = position.z };

            Debug.Log("JoinRequest req=" + req);

            var reply = await client.JoinAsync(req);

            Debug.Log("JoinResponse rep=" + reply);

            Vector3 joinPosition = new Vector3(reply.Pos.X, 0, reply.Pos.Y);
            int uid = reply.Uid;


            this.playerInstance = Instantiate(this.playerPrefab, joinPosition, Quaternion.identity);
            this.players[uid] = playerInstance;
        }

        private async Task RegisterSyncPos()
        {
            var req = new RegisterSyncPos();

            Debug.Log("RegisterSyncPos req=" + req);

            var responseStream = client.SyncPos().ResponseStream;

            while (await responseStream.MoveNext(cts.Token))
            {
                var rep = responseStream.Current;
                Debug.Log("PushSyncPos rep=" + rep);
                this.OnPushSyncPos(rep);
            }
        }

        private void OnPushSyncPos(PushSyncPos pushSyncPos)
        {
            int uid = pushSyncPos.Uid;
            Vector3 position = new Vector3(pushSyncPos.Pos.X, 0, pushSyncPos.Pos.Y);

            if (players.TryGetValue(uid, out PlayerController playerController))
            {
                playerController.transform.position = position;
            }
            else
            {
                this.players[uid] = Instantiate(this.playerPrefab, position, Quaternion.identity);
            }
        }

        private async void RequestMove(Vector3 target)
        {
            var req = new MoveRequest();
            req.Pos = new Position() { X = target.x, Y = target.z };

            Debug.Log("MoveRequest req=" + req);

            var reply = await client.MoveAsync(req);

            Debug.Log("MoveResponse rep=" + reply);

            playerInstance.currentTarget = new Vector3(reply.Pos.X, 0, reply.Pos.Y);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouse = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mouse);
                Plane plane = new Plane(Vector3.up, 0);

                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 hit = ray.GetPoint(enter);
                    this.RequestMove(hit);
                }
            }
        }
    }
}
