using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace leviathan_server
{
    class ServerState
    {
        Server server;
        internal Dictionary<Guid, Delegate> invoke;
        public ServerState(Server server)
        {
            this.server = server;
            invoke = new Dictionary<Guid, Delegate>
			{
				{ Utils.GenerateGuid("Login"), new Func<ServerState, object[], Task<byte[]>>(server.Login) },
				{ Utils.GenerateGuid("CreateAccount"), new Func<ServerState, object[], Task<byte[]>>(server.CreateAccount) },
                { Utils.GenerateGuid("RequestVerificationEmail"), new Func<ServerState, object[], Task<byte[]>>(server.RequestVerificationEmail) },
                { Utils.GenerateGuid("RequestPasswordReset"), new Func<ServerState, object[], Task<byte[]>>(server.RequestPasswordReset) },
                { Utils.GenerateGuid("ResetPassword"), new Func<ServerState, object[], Task<byte[]>>(server.ResetPassword) }
			};
        }

        internal void AddLoggedInInvocations()
        {
            invoke.Add(Utils.GenerateGuid("Join"), new Func<ServerState, object[], Task<byte[]>>(server.Join));
            invoke.Add(Utils.GenerateGuid("WatchReplay"), new Func<ServerState, object[], Task<byte[]>>(server.WatchReplay));
        }
    }
}