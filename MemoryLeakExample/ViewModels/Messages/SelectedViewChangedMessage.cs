using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MemoryLeakExample.ViewModels.Messages
{
    public class SelectedViewChangedMessage : ValueChangedMessage<byte>
    {
        public SelectedViewChangedMessage(byte selectedViewIndex) : base(selectedViewIndex)
        {
            
        }
    }
}
