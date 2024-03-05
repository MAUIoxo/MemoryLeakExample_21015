using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MemoryLeakExample.ViewModels.Messages
{
    public class RefreshDataBaseMessage : ValueChangedMessage<bool>
    {
        public RefreshDataBaseMessage(bool value) : base(value)
        {
            
        }
    }
}
