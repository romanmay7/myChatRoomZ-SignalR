using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using myChatRoomZ.Data.Models;
using myChatRoomZ.Services;

namespace myChatRoomZ.SignalRHub
{
    public class ChatHub:Hub
    {
        private readonly IChatGroupService _chatGroupService;

        public ChatHub(IChatGroupService chatGroupService)
        {
            _chatGroupService = chatGroupService;
        }
        public async Task SendMessage(string name,string text, string channelId)
        {
            var message = new ChatMessage
            {
                SenderName = name,
                Text = text,
                SentAt = DateTimeOffset.UtcNow
            };
            //Broadcast to all Clients,connected to Specific Channel(Group) 
            //The Name the function,that we're invoking on the Client:"RecieveMessage"
            await Clients.Group(channelId).SendAsync("RecieveMessage", message);
        }

        public async Task JoinChannel(string name,string channelId)
        {
            //1.When Client Joins the Channel we will first remove him from other Group on the Hub
            string connected_channelId =_chatGroupService.RemoveConnectionfromGroups(Context.ConnectionId);
            if (connected_channelId != null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, connected_channelId);

            //2.When Client joins the Channel we will add him to appropriate Group on the Hub
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
            _chatGroupService.AddConnectiontoGroup(Context.ConnectionId, channelId);

            //3.Broadcast to all Clients,connected to Specific Channel(Group) 
            //The Name the function,that we're invoking on the Client:"JoinChannel"
            await Clients.Group(channelId).SendAsync("JoinChannel", name);
        }
    }
}
