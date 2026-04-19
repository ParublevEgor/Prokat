namespace Prokat.API.Services
{
    /// <summary>Снимок последнего удалённого пользователя для отмены (один сервер, один администратор).</summary>
    public sealed class UserDeletionSnapshot
    {
        public required string Login { get; init; }
        public required string PasswordHash { get; init; }
        public required string Role { get; init; }
        public int? ClientId { get; init; }
    }

    public static class AdminDeletionUndo
    {
        private static readonly object Sync = new();
        private static UserDeletionSnapshot? _last;

        public static void Remember(UserDeletionSnapshot snapshot)
        {
            lock (Sync) { _last = snapshot; }
        }

        public static bool CanUndo
        {
            get
            {
                lock (Sync) { return _last != null; }
            }
        }

        public static UserDeletionSnapshot? Peek()
        {
            lock (Sync) { return _last; }
        }

        public static void Clear()
        {
            lock (Sync) { _last = null; }
        }
    }
}
