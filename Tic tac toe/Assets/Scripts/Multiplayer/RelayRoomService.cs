using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace TicTacToe.Multiplayer
{
    // Tạo / tham gia phòng qua Unity Relay bằng mã phòng (join code)
    public static class RelayRoomService
    {
        // "udp" ít bị chặn/lỗi bắt tay hơn "dtls" (mã hóa); đổi lại "dtls" khi build thật nếu cần bảo mật
        private const string ConnectionType = "udp";

        public static async Task EnsureSignedInAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                // Mỗi tiến trình dùng một profile riêng: nhiều bản chạy trên cùng một máy (Multiplayer Play Mode,
                // nhiều bản build) sẽ không dùng chung một tài khoản ẩn danh, nếu không Relay báo "allocation ID client mismatch"
                InitializationOptions options = new InitializationOptions();
                options.SetProfile("p" + System.Diagnostics.Process.GetCurrentProcess().Id);
                await UnityServices.InitializeAsync(options);
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }

        // Host: tạo allocation trên Relay, cấu hình transport và trả về mã phòng
        public static async Task<string> CreateRoomAsync()
        {
            await EnsureSignedInAsync();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MultiplayerConstants.MAX_PLAYERS - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerEndpoint endpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == ConnectionType);
            GetTransport().SetRelayServerData(
                endpoint.Host,
                (ushort)endpoint.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData,
                endpoint.Secure);

            return joinCode;
        }

        // Client: dùng mã phòng để cấu hình transport (ném exception nếu mã sai)
        public static async Task JoinRoomAsync(string joinCode)
        {
            await EnsureSignedInAsync();

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerEndpoint endpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == ConnectionType);
            GetTransport().SetRelayServerData(
                endpoint.Host,
                (ushort)endpoint.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData,
                endpoint.Secure);
        }

        private static UnityTransport GetTransport()
        {
            return NetworkManager.Singleton.GetComponent<UnityTransport>();
        }
    }
}
