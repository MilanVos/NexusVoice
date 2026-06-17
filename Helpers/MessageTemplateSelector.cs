using System.Windows;
using System.Windows.Controls;
using NexusVoice.Models;

namespace NexusVoice.Helpers;

public class MessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? DutchTemplate { get; set; }
    public DataTemplate? GermanTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is ConversationMessage msg)
            return msg.Speaker == Speaker.Dutch ? DutchTemplate : GermanTemplate;
        return base.SelectTemplate(item, container);
    }
}
