/* This file is part of eStreamChat.
 * 
 * eStreamChat is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version. 
 * 
 * eStreamChat is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with eStreamChat. If not, see <http://www.gnu.org/licenses/>.
 */

namespace eStreamChat.Interfaces
{
    public enum MessageTypeEnum
    {
        System = 1,
        User = 2,
        Status = 3,
        UserJoined = 4,
        UserLeft = 5,
        SendFile = 6,
        SendImageFile = 7,
        Kicked = 8, //used also for banned
        VideoBroadcast = 9,
        StopVideoBroadcast = 10,
        //ChatRequest = 11,
        RequestAccepted = 12,
        RequestDeclined = 13
    }

    public class MessageFormatOptions
    {
        public bool Bold { get; set; }
        public string? Color { get; set; }
        public string? FontName { get; set; }
        public int FontSize { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
    }

    public class Message
    {
        public string? Content { get; set; }
        public MessageFormatOptions? FormatOptions { get; set; }
        public string? FromUserId { get; set; }
        public MessageTypeEnum MessageType { get; set; }
        public long Timestamp { get; set; }
        public string? ToUserId { get; set; }
    }

    public class Broadcast
    {
        public string? Guid { get; set; }
        public string? ReceiverId { get; set; }
        public string? SenderId { get; set; }
    }
}