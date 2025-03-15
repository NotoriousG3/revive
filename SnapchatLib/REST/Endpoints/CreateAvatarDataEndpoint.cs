using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Extras;
using SnapchatLib.REST.Models;
using SnapProto.Snapchat.Bitmoji.Api;

namespace SnapchatLib.REST.Endpoints;

public interface ICreateAvatarDataEndpoint
{
    Task Black_BlackHaired_MaleMoji();
    Task White_BlackHaired_FemaleMoji();
    Task White_BlackHaired_FemaleMoji2();
    Task White_BlackHaired_FemaleMoji3();
    Task White_BlackHaired_FemaleMoji4();
    Task White_BrownHaired_FemaleMoji();
    Task White_BrownHaired_FemaleMoji2();
    Task White_BrownHaired_FemaleMoji3();
    Task White_BrownHaired_FemaleMoji4();
    Task White_BlondeHaired_FemaleMoji();
    Task White_BlondeHaired_FemaleMoji2();
    Task White_BlondeHaired_FemaleMoji3();
    Task White_BlondeHaired_FemaleMoji4();
    Task CreateBitmoji(bool male, int style, int body, int bottom, int bottom_tone1, int bottom_tone10, int bottom_tone2, int bottom_tone3, int bottom_tone4, int bottom_tone5, int bottom_tone6, int bottom_tone7, int bottom_tone8, int bottom_tone9, int brow, int clothing_type, int ear, int eyelash, int face_proportion, int footwear, int footwear_tone1, int footwear_tone10, int footwear_tone2, int footwear_tone3, int footwear_tone4, int footwear_tone5, int footwear_tone6, int footwear_tone7, int footwear_tone8, int footwear_tone9, int hair, int hair_tone, int is_tucked, int jaw, int mouth, int nose, int pupil, int pupil_tone, int skin_tone, int sock, int sock_tone1, int sock_tone2, int sock_tone3, int sock_tone4, int top, int top_tone1, int top_tone10, int top_tone2, int top_tone3, int top_tone4, int top_tone5, int top_tone6, int top_tone7, int top_tone8, int top_tone9);
}

internal class CreateAvatarDataEndpoint : EndpointAccessor, ICreateAvatarDataEndpoint
{
    public static readonly EndpointInfo CreateAvatarData_Endpoint = new ()
    {
        Url = "/bitmoji-api/avatar-service/create-avatar-data",
        BaseEndpoint = RequestConfigurator.ApiAWSEast1Endpoint,
        Requirements = EndpointRequirements.RequestToken | EndpointRequirements.Username | EndpointRequirements.AcceptEncoding | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.OSUserAgent | EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.ParamsAsHeaders
    };
    public static readonly EndpointInfo Update3DProfile_Endpoint = new()
    {
        Url = "/bitmoji-api/avatar-service/update-3d-profile",
        BaseEndpoint = RequestConfigurator.ApiAWSEast1Endpoint,
        Requirements = EndpointRequirements.RequestToken | EndpointRequirements.Username | EndpointRequirements.AcceptEncoding | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.OSUserAgent | EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.ParamsAsHeaders
    };
    public CreateAvatarDataEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    private HttpContent CreateStream(string base64)
    {
        var stream = new MemoryStream(Convert.FromBase64String(base64));
        
        var streamContent = new StreamContent(stream);
        streamContent.Headers.Add("Content-Type", "application/x-protobuf");
        return streamContent;
    }

    public async Task Black_BlackHaired_MaleMoji()
    {
        using var content = CreateStream("CP///////////wESqgUIARAFGhEKCnB1cGlsX3RvbmUQ+vTpAxoQCgl0b3BfdG9uZTQQ+vXrBxoTCgxib3R0b21fdG9uZTUQ16/fBhoKCgVwdXBpbBDoEBoQCgloYWlyX3RvbmUQqtacARoMCghleWVfc2l6ZRAAGgoKBmJyZWFzdBAAGhMKDGJvdHRvbV90b25lOBDly5cHGggKBGJvZHkQABoQCgl0b3BfdG9uZTkQltS0AhoJCgRicm93EIIMGhMKDGJvdHRvbV90b25lMRD69esHGhQKDWJvdHRvbV90b25lMTAQ1ejxBBoQCgl0b3BfdG9uZTUQ16/fBhoICgNqYXcQ7QoaEwoMYm90dG9tX3RvbmU0EPr16wcaEAoJdG9wX3RvbmUxEPr16wcaEwoPZmFjZV9wcm9wb3J0aW9uEAEaCQoEbm9zZRCcCxoTCgxib3R0b21fdG9uZTcQ5cuXBxoRCgp0b3BfdG9uZTEwENXo8QQaEAoJdG9wX3RvbmU2EIfLsgcaCQoEaGFpchCXChoPCgtleWVfc3BhY2luZxAAGhEKDWNsb3RoaW5nX3R5cGUQARoQCgl0b3BfdG9uZTIQ+vXrBxoHCgN0b3AQNhoKCgVtb3V0aBChEhoICgNlYXIQkAsaEwoMYm90dG9tX3RvbmUzEPr16wcaDAoIZm9vdHdlYXIQABoQCgl0b3BfdG9uZTcQ5cuXBxoTCgxib3R0b21fdG9uZTYQh8uyBxoQCglza2luX3RvbmUQwJTSBRoQCgl0b3BfdG9uZTMQ+vXrBxoICgNleWUQygwaEwoMYm90dG9tX3RvbmU5EJbUtAIaDQoJaXNfdHVja2VkEAAaCwoGYm90dG9tEIkBGhAKCXRvcF90b25lOBDly5cHGhMKDGJvdHRvbV90b25lMhD69esH");
        await Send(CreateAvatarData_Endpoint, content);
    }
    public async Task CreateBitmoji(bool male, int style, int body, int bottom, int bottom_tone1, int bottom_tone10, int bottom_tone2, int bottom_tone3, int bottom_tone4, int bottom_tone5, int bottom_tone6, int bottom_tone7, int bottom_tone8, int bottom_tone9, int brow, int clothing_type, int ear, int eyelash, int face_proportion, int footwear, int footwear_tone1, int footwear_tone10, int footwear_tone2, int footwear_tone3, int footwear_tone4, int footwear_tone5, int footwear_tone6, int footwear_tone7, int footwear_tone8, int footwear_tone9, int hair, int hair_tone, int is_tucked, int jaw, int mouth, int nose, int pupil, int pupil_tone, int skin_tone, int sock, int sock_tone1, int sock_tone2, int sock_tone3, int sock_tone4, int top, int top_tone1, int top_tone10, int top_tone2, int top_tone3, int top_tone4, int top_tone5, int top_tone6, int top_tone7, int top_tone8, int top_tone9)
    {
        int gender;
        if (male)
        {
            gender = 1;
        }
        else
        {
            gender = 2;
        }
        AvatarData avatarData = new AvatarData();
        avatarData.Gender = gender;
        avatarData.Style = style;
        avatarData.OptionIds.Add("body", body);
        avatarData.OptionIds.Add("bottom", bottom);
        avatarData.OptionIds.Add("bottom_tone1", bottom_tone1);
        avatarData.OptionIds.Add("bottom_tone10", bottom_tone10);
        avatarData.OptionIds.Add("bottom_tone2", bottom_tone2);
        avatarData.OptionIds.Add("bottom_tone3", bottom_tone3);
        avatarData.OptionIds.Add("bottom_tone4", bottom_tone4);
        avatarData.OptionIds.Add("bottom_tone5", bottom_tone5);
        avatarData.OptionIds.Add("bottom_tone6", bottom_tone6);
        avatarData.OptionIds.Add("bottom_tone7", bottom_tone7);
        avatarData.OptionIds.Add("bottom_tone8", bottom_tone8);
        avatarData.OptionIds.Add("bottom_tone9", bottom_tone9);
        avatarData.OptionIds.Add("brow", brow);
        avatarData.OptionIds.Add("clothing_type", clothing_type);
        avatarData.OptionIds.Add("ear", ear);
        avatarData.OptionIds.Add("eyelash", eyelash);
        avatarData.OptionIds.Add("face_proportion", face_proportion);
        avatarData.OptionIds.Add("footwear", footwear);
        avatarData.OptionIds.Add("footwear_tone1", footwear_tone1);
        avatarData.OptionIds.Add("footwear_tone10", footwear_tone10);
        avatarData.OptionIds.Add("footwear_tone2", footwear_tone2);
        avatarData.OptionIds.Add("footwear_tone3", footwear_tone3);
        avatarData.OptionIds.Add("footwear_tone4", footwear_tone4);
        avatarData.OptionIds.Add("footwear_tone5", footwear_tone5);
        avatarData.OptionIds.Add("footwear_tone6", footwear_tone6);
        avatarData.OptionIds.Add("footwear_tone7", footwear_tone7);
        avatarData.OptionIds.Add("footwear_tone8", footwear_tone8);
        avatarData.OptionIds.Add("footwear_tone9", footwear_tone9);
        avatarData.OptionIds.Add("hair", hair);
        avatarData.OptionIds.Add("hair_tone", hair_tone);
        avatarData.OptionIds.Add("is_tucked", is_tucked);
        avatarData.OptionIds.Add("jaw", jaw);
        avatarData.OptionIds.Add("mouth", mouth);
        avatarData.OptionIds.Add("nose", nose);
        avatarData.OptionIds.Add("pupil_tone", pupil_tone);
        avatarData.OptionIds.Add("skin_tone", skin_tone);
        avatarData.OptionIds.Add("sock", sock);
        avatarData.OptionIds.Add("sock_tone1", sock_tone1);
        avatarData.OptionIds.Add("sock_tone2", sock_tone2);
        avatarData.OptionIds.Add("sock_tone3", sock_tone3);
        avatarData.OptionIds.Add("sock_tone4", sock_tone4);
        avatarData.OptionIds.Add("top", top);
        avatarData.OptionIds.Add("top_tone1", top_tone1);
        avatarData.OptionIds.Add("top_tone10", top_tone10);
        avatarData.OptionIds.Add("top_tone2", top_tone2);
        avatarData.OptionIds.Add("top_tone3", top_tone3);
        avatarData.OptionIds.Add("top_tone4", top_tone4);
        avatarData.OptionIds.Add("top_tone5", top_tone5);
        avatarData.OptionIds.Add("top_tone6", top_tone6);
        avatarData.OptionIds.Add("top_tonep7", top_tone7);
        avatarData.OptionIds.Add("top_tone8", top_tone8);
        avatarData.OptionIds.Add("top_tone9", top_tone9);
        var CreateAvatarDataRequest_pb2 = new CreateAvatarDataRequest
        {
            TouVersion = -1,
            AvatarData = avatarData,
        };

        // send data via HTTP
        using var ByteArrayContent = new ByteArrayContent(CreateAvatarDataRequest_pb2.ToByteArray());
        ByteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");

        await Send(CreateAvatarData_Endpoint, ByteArrayContent);
    }
    public async Task White_BlackHaired_FemaleMoji()
    {
        using var content = CreateStream("CP///////////wESoQkIAhAFGhAKCXNraW5fdG9uZRCI2cYHGg8KCWhhaXJfdG9uZRCevHwaCQoEaGFpchD3FhoICgNqYXcQ/woaCQoEYnJvdxCmDBoICgNleWUQ0wwaCgoFcHVwaWwQ6BAaEQoKcHVwaWxfdG9uZRC11+UBGgkKBG5vc2UQ0wsaCgoFbW91dGgQpRIaCAoDZWFyEJYLGggKBGJvZHkQBxoTCg9mYWNlX3Byb3BvcnRpb24QARoMCgdleWVsYXNoEOkRGhEKDWNsb3RoaW5nX3R5cGUQARoICgNoYXQQwUMaEAoJaGF0X3RvbmUxEJ2OyAYaEAoJaGF0X3RvbmUyEJ2OyAYaEAoJaGF0X3RvbmUzEJ2OyAYaEAoJaGF0X3RvbmU0EJ2OyAYaEAoJaGF0X3RvbmU1EJ2OyAYaEAoJaGF0X3RvbmU2EJ2OyAYaEAoJaGF0X3RvbmU3EJ2OyAYaEAoJaGF0X3RvbmU4EJ2OyAYaEAoJaGF0X3RvbmU5EJ2OyAYaCAoDdG9wEJUHGgsKBmJvdHRvbRCaBxoNCghmb290d2VhchCYBxoJCgRzb2NrEKcCGhAKCXRvcF90b25lMRDP8fsHGhAKCXRvcF90b25lMhDP8fsHGhAKCXRvcF90b25lMxDP8fsHGhAKCXRvcF90b25lNBDP8fsHGhAKCXRvcF90b25lNRD69esHGhAKCXRvcF90b25lNhCA+fEDGhAKCXRvcF90b25lNxD69esHGhAKCXRvcF90b25lOBCKob8GGhAKCXRvcF90b25lORDx2rUDGhEKCnRvcF90b25lMTAQ+fz9AxoTCgxib3R0b21fdG9uZTEQ2PnuBBoTCgxib3R0b21fdG9uZTIQ5abBBxoSCgxib3R0b21fdG9uZTMQ0aYtGhMKDGJvdHRvbV90b25lNBDt1dsHGhMKDGJvdHRvbV90b25lNRDo4PUBGhMKDGJvdHRvbV90b25lNhCWpeYEGhMKDGJvdHRvbV90b25lNxDt1dsHGhMKDGJvdHRvbV90b25lOBCEvc0BGhMKDGJvdHRvbV90b25lORDRgOECGhQKDWJvdHRvbV90b25lMTAQvYnHBhoVCg5mb290d2Vhcl90b25lMRC7i7cHGhUKDmZvb3R3ZWFyX3RvbmUyEJvBtgYaFQoOZm9vdHdlYXJfdG9uZTMQu4u3BxoVCg5mb290d2Vhcl90b25lNBC7i7cHGhUKDmZvb3R3ZWFyX3RvbmU1EN3r+wcaFQoOZm9vdHdlYXJfdG9uZTYQg93mBhoVCg5mb290d2Vhcl90b25lNxDd6/sHGhUKDmZvb3R3ZWFyX3RvbmU4EKrHsgYaFQoOZm9vdHdlYXJfdG9uZTkQu4u3BxoWCg9mb290d2Vhcl90b25lMTAQzoDKBRoRCgpzb2NrX3RvbmUxEOvhzwcaEQoKc29ja190b25lMhDTiOEBGhEKCnNvY2tfdG9uZTMQ6+HPBxoRCgpzb2NrX3RvbmU0EOvhzwcaDQoJaXNfdHVja2VkEAEaFQoOZXllc2hhZG93X3RvbmUQqPm1BxoRCgpibHVzaF90b25lEKDdxQc=");
        await Send(CreateAvatarData_Endpoint, content);
    }
    public async Task White_BlackHaired_FemaleMoji2()
    {
        using var content = CreateStream("CP///////////wESoQkIAhAFGhAKCXNraW5fdG9uZRCI2cYHGg8KCWhhaXJfdG9uZRCevHwaCQoEaGFpchD3FhoICgNqYXcQ/woaCQoEYnJvdxCmDBoICgNleWUQ0wwaCgoFcHVwaWwQ6BAaEQoKcHVwaWxfdG9uZRC11+UBGgkKBG5vc2UQ0wsaCgoFbW91dGgQpRIaCAoDZWFyEJYLGggKBGJvZHkQBxoTCg9mYWNlX3Byb3BvcnRpb24QARoMCgdleWVsYXNoEOkRGhEKDWNsb3RoaW5nX3R5cGUQARoICgNoYXQQwUMaEAoJaGF0X3RvbmUxEJ2OyAYaEAoJaGF0X3RvbmUyEJ2OyAYaEAoJaGF0X3RvbmUzEJ2OyAYaEAoJaGF0X3RvbmU0EJ2OyAYaEAoJaGF0X3RvbmU1EJ2OyAYaEAoJaGF0X3RvbmU2EJ2OyAYaEAoJaGF0X3RvbmU3EJ2OyAYaEAoJaGF0X3RvbmU4EJ2OyAYaEAoJaGF0X3RvbmU5EJ2OyAYaCAoDdG9wEJUHGgsKBmJvdHRvbRCaBxoNCghmb290d2VhchCYBxoJCgRzb2NrEKcCGhAKCXRvcF90b25lMRDP8fsHGhAKCXRvcF90b25lMhDP8fsHGhAKCXRvcF90b25lMxDP8fsHGhAKCXRvcF90b25lNBDP8fsHGhAKCXRvcF90b25lNRD69esHGhAKCXRvcF90b25lNhCA+fEDGhAKCXRvcF90b25lNxD69esHGhAKCXRvcF90b25lOBCKob8GGhAKCXRvcF90b25lORDx2rUDGhEKCnRvcF90b25lMTAQ+fz9AxoTCgxib3R0b21fdG9uZTEQ2PnuBBoTCgxib3R0b21fdG9uZTIQ5abBBxoSCgxib3R0b21fdG9uZTMQ0aYtGhMKDGJvdHRvbV90b25lNBDt1dsHGhMKDGJvdHRvbV90b25lNRDo4PUBGhMKDGJvdHRvbV90b25lNhCWpeYEGhMKDGJvdHRvbV90b25lNxDt1dsHGhMKDGJvdHRvbV90b25lOBCEvc0BGhMKDGJvdHRvbV90b25lORDRgOECGhQKDWJvdHRvbV90b25lMTAQvYnHBhoVCg5mb290d2Vhcl90b25lMRC7i7cHGhUKDmZvb3R3ZWFyX3RvbmUyEJvBtgYaFQoOZm9vdHdlYXJfdG9uZTMQu4u3BxoVCg5mb290d2Vhcl90b25lNBC7i7cHGhUKDmZvb3R3ZWFyX3RvbmU1EN3r+wcaFQoOZm9vdHdlYXJfdG9uZTYQg93mBhoVCg5mb290d2Vhcl90b25lNxDd6/sHGhUKDmZvb3R3ZWFyX3RvbmU4EKrHsgYaFQoOZm9vdHdlYXJfdG9uZTkQu4u3BxoWCg9mb290d2Vhcl90b25lMTAQzoDKBRoRCgpzb2NrX3RvbmUxEOvhzwcaEQoKc29ja190b25lMhDTiOEBGhEKCnNvY2tfdG9uZTMQ6+HPBxoRCgpzb2NrX3RvbmU0EOvhzwcaDQoJaXNfdHVja2VkEAEaFQoOZXllc2hhZG93X3RvbmUQqPm1BxoRCgpibHVzaF90b25lEKDdxQc=");
        await Send(CreateAvatarData_Endpoint, content);
        var Update3DProfile_parameters = new Dictionary<string, string>();
        Update3DProfile_parameters.Add("background_id", "383755736");
        Update3DProfile_parameters.Add("scene_id", "383755736");
        await Send(Update3DProfile_Endpoint, Update3DProfile_parameters);
    }
    public async Task White_BlackHaired_FemaleMoji3()
    {
        using var content = CreateStream("CP///////////wESoQkIAhAFGhAKCXNraW5fdG9uZRCI2cYHGg8KCWhhaXJfdG9uZRCevHwaCQoEaGFpchD3FhoICgNqYXcQ/woaCQoEYnJvdxCmDBoICgNleWUQ0wwaCgoFcHVwaWwQ6BAaEQoKcHVwaWxfdG9uZRC11+UBGgkKBG5vc2UQ0wsaCgoFbW91dGgQpRIaCAoDZWFyEJYLGggKBGJvZHkQBxoTCg9mYWNlX3Byb3BvcnRpb24QARoMCgdleWVsYXNoEOkRGhEKDWNsb3RoaW5nX3R5cGUQARoICgNoYXQQwUMaEAoJaGF0X3RvbmUxEJ2OyAYaEAoJaGF0X3RvbmUyEJ2OyAYaEAoJaGF0X3RvbmUzEJ2OyAYaEAoJaGF0X3RvbmU0EJ2OyAYaEAoJaGF0X3RvbmU1EJ2OyAYaEAoJaGF0X3RvbmU2EJ2OyAYaEAoJaGF0X3RvbmU3EJ2OyAYaEAoJaGF0X3RvbmU4EJ2OyAYaEAoJaGF0X3RvbmU5EJ2OyAYaCAoDdG9wEJUHGgsKBmJvdHRvbRCaBxoNCghmb290d2VhchCYBxoJCgRzb2NrEKcCGhAKCXRvcF90b25lMRDP8fsHGhAKCXRvcF90b25lMhDP8fsHGhAKCXRvcF90b25lMxDP8fsHGhAKCXRvcF90b25lNBDP8fsHGhAKCXRvcF90b25lNRD69esHGhAKCXRvcF90b25lNhCA+fEDGhAKCXRvcF90b25lNxD69esHGhAKCXRvcF90b25lOBCKob8GGhAKCXRvcF90b25lORDx2rUDGhEKCnRvcF90b25lMTAQ+fz9AxoTCgxib3R0b21fdG9uZTEQ2PnuBBoTCgxib3R0b21fdG9uZTIQ5abBBxoSCgxib3R0b21fdG9uZTMQ0aYtGhMKDGJvdHRvbV90b25lNBDt1dsHGhMKDGJvdHRvbV90b25lNRDo4PUBGhMKDGJvdHRvbV90b25lNhCWpeYEGhMKDGJvdHRvbV90b25lNxDt1dsHGhMKDGJvdHRvbV90b25lOBCEvc0BGhMKDGJvdHRvbV90b25lORDRgOECGhQKDWJvdHRvbV90b25lMTAQvYnHBhoVCg5mb290d2Vhcl90b25lMRC7i7cHGhUKDmZvb3R3ZWFyX3RvbmUyEJvBtgYaFQoOZm9vdHdlYXJfdG9uZTMQu4u3BxoVCg5mb290d2Vhcl90b25lNBC7i7cHGhUKDmZvb3R3ZWFyX3RvbmU1EN3r+wcaFQoOZm9vdHdlYXJfdG9uZTYQg93mBhoVCg5mb290d2Vhcl90b25lNxDd6/sHGhUKDmZvb3R3ZWFyX3RvbmU4EKrHsgYaFQoOZm9vdHdlYXJfdG9uZTkQu4u3BxoWCg9mb290d2Vhcl90b25lMTAQzoDKBRoRCgpzb2NrX3RvbmUxEOvhzwcaEQoKc29ja190b25lMhDTiOEBGhEKCnNvY2tfdG9uZTMQ6+HPBxoRCgpzb2NrX3RvbmU0EOvhzwcaDQoJaXNfdHVja2VkEAEaFQoOZXllc2hhZG93X3RvbmUQqPm1BxoRCgpibHVzaF90b25lEKDdxQc=");
        await Send(CreateAvatarData_Endpoint, content);
        var Update3DProfile_parameters = new Dictionary<string, string>();
        Update3DProfile_parameters.Add("background_id", "319087826");
        Update3DProfile_parameters.Add("scene_id", "708455430");
        await Send(Update3DProfile_Endpoint, Update3DProfile_parameters);
    }
    public async Task White_BlackHaired_FemaleMoji4()
    {
        using var content = CreateStream("CP///////////wESoQkIAhAFGhAKCXNraW5fdG9uZRCI2cYHGg8KCWhhaXJfdG9uZRCevHwaCQoEaGFpchD3FhoICgNqYXcQ/woaCQoEYnJvdxCmDBoICgNleWUQ0wwaCgoFcHVwaWwQ6BAaEQoKcHVwaWxfdG9uZRC11+UBGgkKBG5vc2UQ0wsaCgoFbW91dGgQpRIaCAoDZWFyEJYLGggKBGJvZHkQBxoTCg9mYWNlX3Byb3BvcnRpb24QARoMCgdleWVsYXNoEOkRGhEKDWNsb3RoaW5nX3R5cGUQARoICgNoYXQQwUMaEAoJaGF0X3RvbmUxEJ2OyAYaEAoJaGF0X3RvbmUyEJ2OyAYaEAoJaGF0X3RvbmUzEJ2OyAYaEAoJaGF0X3RvbmU0EJ2OyAYaEAoJaGF0X3RvbmU1EJ2OyAYaEAoJaGF0X3RvbmU2EJ2OyAYaEAoJaGF0X3RvbmU3EJ2OyAYaEAoJaGF0X3RvbmU4EJ2OyAYaEAoJaGF0X3RvbmU5EJ2OyAYaCAoDdG9wEJUHGgsKBmJvdHRvbRCaBxoNCghmb290d2VhchCYBxoJCgRzb2NrEKcCGhAKCXRvcF90b25lMRDP8fsHGhAKCXRvcF90b25lMhDP8fsHGhAKCXRvcF90b25lMxDP8fsHGhAKCXRvcF90b25lNBDP8fsHGhAKCXRvcF90b25lNRD69esHGhAKCXRvcF90b25lNhCA+fEDGhAKCXRvcF90b25lNxD69esHGhAKCXRvcF90b25lOBCKob8GGhAKCXRvcF90b25lORDx2rUDGhEKCnRvcF90b25lMTAQ+fz9AxoTCgxib3R0b21fdG9uZTEQ2PnuBBoTCgxib3R0b21fdG9uZTIQ5abBBxoSCgxib3R0b21fdG9uZTMQ0aYtGhMKDGJvdHRvbV90b25lNBDt1dsHGhMKDGJvdHRvbV90b25lNRDo4PUBGhMKDGJvdHRvbV90b25lNhCWpeYEGhMKDGJvdHRvbV90b25lNxDt1dsHGhMKDGJvdHRvbV90b25lOBCEvc0BGhMKDGJvdHRvbV90b25lORDRgOECGhQKDWJvdHRvbV90b25lMTAQvYnHBhoVCg5mb290d2Vhcl90b25lMRC7i7cHGhUKDmZvb3R3ZWFyX3RvbmUyEJvBtgYaFQoOZm9vdHdlYXJfdG9uZTMQu4u3BxoVCg5mb290d2Vhcl90b25lNBC7i7cHGhUKDmZvb3R3ZWFyX3RvbmU1EN3r+wcaFQoOZm9vdHdlYXJfdG9uZTYQg93mBhoVCg5mb290d2Vhcl90b25lNxDd6/sHGhUKDmZvb3R3ZWFyX3RvbmU4EKrHsgYaFQoOZm9vdHdlYXJfdG9uZTkQu4u3BxoWCg9mb290d2Vhcl90b25lMTAQzoDKBRoRCgpzb2NrX3RvbmUxEOvhzwcaEQoKc29ja190b25lMhDTiOEBGhEKCnNvY2tfdG9uZTMQ6+HPBxoRCgpzb2NrX3RvbmU0EOvhzwcaDQoJaXNfdHVja2VkEAEaFQoOZXllc2hhZG93X3RvbmUQqPm1BxoRCgpibHVzaF90b25lEKDdxQc=");
        await Send(CreateAvatarData_Endpoint, content);
        var Update3DProfile_parameters = new Dictionary<string, string>();
        Update3DProfile_parameters.Add("background_id", "290927685");
        Update3DProfile_parameters.Add("scene_id", "582513516");
        await Send(Update3DProfile_Endpoint, Update3DProfile_parameters);
    }
    public async Task White_BrownHaired_FemaleMoji()
    {
        using var content = CreateStream("CP///////////wESvAcIAhAFGhAKCXNraW5fdG9uZRCI2cYHGhAKCWhhaXJfdG9uZRC6gtUCGgkKBGhhaXIQxBcaCAoDamF3EP8KGgkKBGJyb3cQpgwaCAoDZXllENAMGgoKBXB1cGlsEOgQGhEKCnB1cGlsX3RvbmUQtdflARoJCgRub3NlENMLGgoKBW1vdXRoEKQSGggKA2VhchCWCxoICgRib2R5EAkaEwoPZmFjZV9wcm9wb3J0aW9uEAEaDAoHZXllbGFzaBDpERoICgN0b3AQoQYaCwoGYm90dG9tEKsFGg0KCGZvb3R3ZWFyEPEGGhEKDWNsb3RoaW5nX3R5cGUQARoVChFub3NlcmluZ19ub3N0cmlsTBAbGh4KF25vc2VyaW5nX25vc3RyaWxMX3RvbmUxEIWY/wcaHgoXbm9zZXJpbmdfbm9zdHJpbExfdG9uZTIQhZj/BxoQCgl0b3BfdG9uZTEQq9GdAxoQCgl0b3BfdG9uZTIQ79nDBxoQCgl0b3BfdG9uZTMQ4MjxAxoQCgl0b3BfdG9uZTQQ0KfvBhoQCgl0b3BfdG9uZTUQ4LiJAxoQCgl0b3BfdG9uZTYQh82WBhoQCgl0b3BfdG9uZTcQ/v37BxoQCgl0b3BfdG9uZTgQ+8eGBRoQCgl0b3BfdG9uZTkQuOTMARoRCgp0b3BfdG9uZTEwEO/ZwwcaEwoMYm90dG9tX3RvbmUxENv6xAEaEwoMYm90dG9tX3RvbmUyENv6xAEaEwoMYm90dG9tX3RvbmUzENv6xAEaEwoMYm90dG9tX3RvbmU0ENv6xAEaEwoMYm90dG9tX3RvbmU1EOGd7wUaEwoMYm90dG9tX3RvbmU2ENyIvgYaEwoMYm90dG9tX3RvbmU3EM3oqAEaEwoMYm90dG9tX3RvbmU4EK2k3QQaEwoMYm90dG9tX3RvbmU5EIn9ugcaFAoNYm90dG9tX3RvbmUxMBCj684GGhUKDmZvb3R3ZWFyX3RvbmUxEPr16wcaFQoOZm9vdHdlYXJfdG9uZTIQ1q3bBhoUCg5mb290d2Vhcl90b25lMxCXrlwaFQoOZm9vdHdlYXJfdG9uZTQQ+vXrBxoVCg5mb290d2Vhcl90b25lNRDh3ccHGhUKDmZvb3R3ZWFyX3RvbmU2EPr16wcaFQoOZm9vdHdlYXJfdG9uZTcQ+vXrBxoVCg5mb290d2Vhcl90b25lOBCr2uYGGhUKDmZvb3R3ZWFyX3RvbmU5EPr16wcaFQoPZm9vdHdlYXJfdG9uZTEwEJeuXA==");
        await Send(CreateAvatarData_Endpoint, content);
    }
    public async Task White_BrownHaired_FemaleMoji2()
    {
        using var content = CreateStream("CP///////////wESvAcIAhAFGhAKCXNraW5fdG9uZRCI2cYHGhAKCWhhaXJfdG9uZRC6gtUCGgkKBGhhaXIQxBcaCAoDamF3EP8KGgkKBGJyb3cQpgwaCAoDZXllENAMGgoKBXB1cGlsEOgQGhEKCnB1cGlsX3RvbmUQtdflARoJCgRub3NlENMLGgoKBW1vdXRoEKQSGggKA2VhchCWCxoICgRib2R5EAkaEwoPZmFjZV9wcm9wb3J0aW9uEAEaDAoHZXllbGFzaBDpERoICgN0b3AQoQYaCwoGYm90dG9tEKsFGg0KCGZvb3R3ZWFyEPEGGhEKDWNsb3RoaW5nX3R5cGUQARoVChFub3NlcmluZ19ub3N0cmlsTBAbGh4KF25vc2VyaW5nX25vc3RyaWxMX3RvbmUxEIWY/wcaHgoXbm9zZXJpbmdfbm9zdHJpbExfdG9uZTIQhZj/BxoQCgl0b3BfdG9uZTEQq9GdAxoQCgl0b3BfdG9uZTIQ79nDBxoQCgl0b3BfdG9uZTMQ4MjxAxoQCgl0b3BfdG9uZTQQ0KfvBhoQCgl0b3BfdG9uZTUQ4LiJAxoQCgl0b3BfdG9uZTYQh82WBhoQCgl0b3BfdG9uZTcQ/v37BxoQCgl0b3BfdG9uZTgQ+8eGBRoQCgl0b3BfdG9uZTkQuOTMARoRCgp0b3BfdG9uZTEwEO/ZwwcaEwoMYm90dG9tX3RvbmUxENv6xAEaEwoMYm90dG9tX3RvbmUyENv6xAEaEwoMYm90dG9tX3RvbmUzENv6xAEaEwoMYm90dG9tX3RvbmU0ENv6xAEaEwoMYm90dG9tX3RvbmU1EOGd7wUaEwoMYm90dG9tX3RvbmU2ENyIvgYaEwoMYm90dG9tX3RvbmU3EM3oqAEaEwoMYm90dG9tX3RvbmU4EK2k3QQaEwoMYm90dG9tX3RvbmU5EIn9ugcaFAoNYm90dG9tX3RvbmUxMBCj684GGhUKDmZvb3R3ZWFyX3RvbmUxEPr16wcaFQoOZm9vdHdlYXJfdG9uZTIQ1q3bBhoUCg5mb290d2Vhcl90b25lMxCXrlwaFQoOZm9vdHdlYXJfdG9uZTQQ+vXrBxoVCg5mb290d2Vhcl90b25lNRDh3ccHGhUKDmZvb3R3ZWFyX3RvbmU2EPr16wcaFQoOZm9vdHdlYXJfdG9uZTcQ+vXrBxoVCg5mb290d2Vhcl90b25lOBCr2uYGGhUKDmZvb3R3ZWFyX3RvbmU5EPr16wcaFQoPZm9vdHdlYXJfdG9uZTEwEJeuXA==");
        await Send(CreateAvatarData_Endpoint, content);
        var Update3DProfile_parameters = new Dictionary<string, string>();
        Update3DProfile_parameters.Add("background_id", "383755736");
        Update3DProfile_parameters.Add("scene_id", "383755736");
        await Send(Update3DProfile_Endpoint, Update3DProfile_parameters);
    }
    public async Task White_BrownHaired_FemaleMoji3()
    {
        using var content = CreateStream("CP///////////wESvAcIAhAFGhAKCXNraW5fdG9uZRCI2cYHGhAKCWhhaXJfdG9uZRC6gtUCGgkKBGhhaXIQxBcaCAoDamF3EP8KGgkKBGJyb3cQpgwaCAoDZXllENAMGgoKBXB1cGlsEOgQGhEKCnB1cGlsX3RvbmUQtdflARoJCgRub3NlENMLGgoKBW1vdXRoEKQSGggKA2VhchCWCxoICgRib2R5EAkaEwoPZmFjZV9wcm9wb3J0aW9uEAEaDAoHZXllbGFzaBDpERoICgN0b3AQoQYaCwoGYm90dG9tEKsFGg0KCGZvb3R3ZWFyEPEGGhEKDWNsb3RoaW5nX3R5cGUQARoVChFub3NlcmluZ19ub3N0cmlsTBAbGh4KF25vc2VyaW5nX25vc3RyaWxMX3RvbmUxEIWY/wcaHgoXbm9zZXJpbmdfbm9zdHJpbExfdG9uZTIQhZj/BxoQCgl0b3BfdG9uZTEQq9GdAxoQCgl0b3BfdG9uZTIQ79nDBxoQCgl0b3BfdG9uZTMQ4MjxAxoQCgl0b3BfdG9uZTQQ0KfvBhoQCgl0b3BfdG9uZTUQ4LiJAxoQCgl0b3BfdG9uZTYQh82WBhoQCgl0b3BfdG9uZTcQ/v37BxoQCgl0b3BfdG9uZTgQ+8eGBRoQCgl0b3BfdG9uZTkQuOTMARoRCgp0b3BfdG9uZTEwEO/ZwwcaEwoMYm90dG9tX3RvbmUxENv6xAEaEwoMYm90dG9tX3RvbmUyENv6xAEaEwoMYm90dG9tX3RvbmUzENv6xAEaEwoMYm90dG9tX3RvbmU0ENv6xAEaEwoMYm90dG9tX3RvbmU1EOGd7wUaEwoMYm90dG9tX3RvbmU2ENyIvgYaEwoMYm90dG9tX3RvbmU3EM3oqAEaEwoMYm90dG9tX3RvbmU4EK2k3QQaEwoMYm90dG9tX3RvbmU5EIn9ugcaFAoNYm90dG9tX3RvbmUxMBCj684GGhUKDmZvb3R3ZWFyX3RvbmUxEPr16wcaFQoOZm9vdHdlYXJfdG9uZTIQ1q3bBhoUCg5mb290d2Vhcl90b25lMxCXrlwaFQoOZm9vdHdlYXJfdG9uZTQQ+vXrBxoVCg5mb290d2Vhcl90b25lNRDh3ccHGhUKDmZvb3R3ZWFyX3RvbmU2EPr16wcaFQoOZm9vdHdlYXJfdG9uZTcQ+vXrBxoVCg5mb290d2Vhcl90b25lOBCr2uYGGhUKDmZvb3R3ZWFyX3RvbmU5EPr16wcaFQoPZm9vdHdlYXJfdG9uZTEwEJeuXA==");
        await Send(CreateAvatarData_Endpoint, content);
        var Update3DProfile_parameters = new Dictionary<string, string>();
        Update3DProfile_parameters.Add("background_id", "319087826");
        Update3DProfile_parameters.Add("scene_id", "708455430");
        await Send(Update3DProfile_Endpoint, Update3DProfile_parameters);
    }
    public async Task White_BrownHaired_FemaleMoji4()
    {
        using var content = CreateStream("CP///////////wESvAcIAhAFGhAKCXNraW5fdG9uZRCI2cYHGhAKCWhhaXJfdG9uZRC6gtUCGgkKBGhhaXIQxBcaCAoDamF3EP8KGgkKBGJyb3cQpgwaCAoDZXllENAMGgoKBXB1cGlsEOgQGhEKCnB1cGlsX3RvbmUQtdflARoJCgRub3NlENMLGgoKBW1vdXRoEKQSGggKA2VhchCWCxoICgRib2R5EAkaEwoPZmFjZV9wcm9wb3J0aW9uEAEaDAoHZXllbGFzaBDpERoICgN0b3AQoQYaCwoGYm90dG9tEKsFGg0KCGZvb3R3ZWFyEPEGGhEKDWNsb3RoaW5nX3R5cGUQARoVChFub3NlcmluZ19ub3N0cmlsTBAbGh4KF25vc2VyaW5nX25vc3RyaWxMX3RvbmUxEIWY/wcaHgoXbm9zZXJpbmdfbm9zdHJpbExfdG9uZTIQhZj/BxoQCgl0b3BfdG9uZTEQq9GdAxoQCgl0b3BfdG9uZTIQ79nDBxoQCgl0b3BfdG9uZTMQ4MjxAxoQCgl0b3BfdG9uZTQQ0KfvBhoQCgl0b3BfdG9uZTUQ4LiJAxoQCgl0b3BfdG9uZTYQh82WBhoQCgl0b3BfdG9uZTcQ/v37BxoQCgl0b3BfdG9uZTgQ+8eGBRoQCgl0b3BfdG9uZTkQuOTMARoRCgp0b3BfdG9uZTEwEO/ZwwcaEwoMYm90dG9tX3RvbmUxENv6xAEaEwoMYm90dG9tX3RvbmUyENv6xAEaEwoMYm90dG9tX3RvbmUzENv6xAEaEwoMYm90dG9tX3RvbmU0ENv6xAEaEwoMYm90dG9tX3RvbmU1EOGd7wUaEwoMYm90dG9tX3RvbmU2ENyIvgYaEwoMYm90dG9tX3RvbmU3EM3oqAEaEwoMYm90dG9tX3RvbmU4EK2k3QQaEwoMYm90dG9tX3RvbmU5EIn9ugcaFAoNYm90dG9tX3RvbmUxMBCj684GGhUKDmZvb3R3ZWFyX3RvbmUxEPr16wcaFQoOZm9vdHdlYXJfdG9uZTIQ1q3bBhoUCg5mb290d2Vhcl90b25lMxCXrlwaFQoOZm9vdHdlYXJfdG9uZTQQ+vXrBxoVCg5mb290d2Vhcl90b25lNRDh3ccHGhUKDmZvb3R3ZWFyX3RvbmU2EPr16wcaFQoOZm9vdHdlYXJfdG9uZTcQ+vXrBxoVCg5mb290d2Vhcl90b25lOBCr2uYGGhUKDmZvb3R3ZWFyX3RvbmU5EPr16wcaFQoPZm9vdHdlYXJfdG9uZTEwEJeuXA==");
        await Send(CreateAvatarData_Endpoint, content);
        var Update3DProfile_parameters = new Dictionary<string, string>();
        Update3DProfile_parameters.Add("background_id", "290927685");
        Update3DProfile_parameters.Add("scene_id", "582513516");
        await Send(Update3DProfile_Endpoint, Update3DProfile_parameters);
    }
    public async Task White_BlondeHaired_FemaleMoji()
    {
        using var content = CreateStream("CP///////////wEStAsIAhAFGhAKCXNraW5fdG9uZRCG4/oHGhAKCWhhaXJfdG9uZRD9kL8HGgkKBGhhaXIQqAoaCAoDamF3EP8KGgkKBGJyb3cQpgwaCAoDZXllENQMGgoKBXB1cGlsEOgQGhEKCnB1cGlsX3RvbmUQtdflARoJCgRub3NlENMLGgoKBW1vdXRoEKQSGggKA2VhchCWCxoICgRib2R5EAcaEwoPZmFjZV9wcm9wb3J0aW9uEAEaDAoHZXllbGFzaBDpERoRCg1jbG90aGluZ190eXBlEAEaGgoTaGFpcl90cmVhdG1lbnRfdG9uZRCl8uwCGhIKDmVhcnJpbmdSX2xvYmUxEAYaGwoUZWFycmluZ1JfbG9iZTFfdG9uZTEQhZj/BxobChRlYXJyaW5nUl9sb2JlMV90b25lMhCFmP8HGhsKFGVhcnJpbmdSX2xvYmUxX3RvbmUzEIWY/wcaGwoUZWFycmluZ1JfbG9iZTFfdG9uZTQQhZj/BxoSCg5lYXJyaW5nTF9sb2JlMRAGGhsKFGVhcnJpbmdMX2xvYmUxX3RvbmUxEIWY/wcaGwoUZWFycmluZ0xfbG9iZTFfdG9uZTIQhZj/BxobChRlYXJyaW5nTF9sb2JlMV90b25lMxCFmP8HGhsKFGVhcnJpbmdMX2xvYmUxX3RvbmU0EIWY/wcaEgoNY2hlZWtfZGV0YWlscxD0DBoICgNoYXQQ10MaEAoJaGF0X3RvbmUxELPmzAEaEAoJaGF0X3RvbmUyELPmzAEaEAoJaGF0X3RvbmUzELPmzAEaEAoJaGF0X3RvbmU0ELPmzAEaEAoJaGF0X3RvbmU1ELPmzAEaEAoJaGF0X3RvbmU2ELPmzAEaEAoJaGF0X3RvbmU3ELPmzAEaEAoJaGF0X3RvbmU4ELPmzAEaEAoJaGF0X3RvbmU5ELPmzAEaCAoDdG9wEP4BGgsKBmJvdHRvbRC0BxoNCghmb290d2VhchD1ARoJCgRzb2NrEKcCGhAKCXRvcF90b25lMRDzwfcFGhAKCXRvcF90b25lMhDzwfcFGhAKCXRvcF90b25lMxDzwfcFGhAKCXRvcF90b25lNBDzwfcFGhAKCXRvcF90b25lNRD54fsGGhAKCXRvcF90b25lNhCjx44FGhAKCXRvcF90b25lNxD877sHGhAKCXRvcF90b25lOBDmj78EGhAKCXRvcF90b25lORDA8MwBGhEKCnRvcF90b25lMTAQgPPJAxoTCgxib3R0b21fdG9uZTEQjZuuBBoTCgxib3R0b21fdG9uZTIQjZuuBBoTCgxib3R0b21fdG9uZTMQjZuuBBoTCgxib3R0b21fdG9uZTQQjZuuBBoTCgxib3R0b21fdG9uZTUQ3LjlAhoTCgxib3R0b21fdG9uZTYQo8eOBRoTCgxib3R0b21fdG9uZTcQw4ePBhoTCgxib3R0b21fdG9uZTgQ3LTJAhoTCgxib3R0b21fdG9uZTkQnsCEARoUCg1ib3R0b21fdG9uZTEwEMWLkwYaFAoOZm9vdHdlYXJfdG9uZTEQnLRsGhQKDmZvb3R3ZWFyX3RvbmUyEJWoUBoUCg5mb290d2Vhcl90b25lMxCctGwaFAoOZm9vdHdlYXJfdG9uZTQQnLRsGhUKDmZvb3R3ZWFyX3RvbmU1EKjOoAEaFQoOZm9vdHdlYXJfdG9uZTYQ7dStAxoVCg5mb290d2Vhcl90b25lNxCozqABGhUKDmZvb3R3ZWFyX3RvbmU4EKHChAEaFQoOZm9vdHdlYXJfdG9uZTkQocKEARoVCg9mb290d2Vhcl90b25lMTAQlKBIGhEKCnNvY2tfdG9uZTEQ6+HPBxoRCgpzb2NrX3RvbmUyENOI4QEaEQoKc29ja190b25lMxDr4c8HGhEKCnNvY2tfdG9uZTQQ6+HPBxoNCglpc190dWNrZWQQAQ==");
        await Send(CreateAvatarData_Endpoint, content);
    }
    public async Task White_BlondeHaired_FemaleMoji2()
    {
        using var content = CreateStream("CP///////////wEStAsIAhAFGhAKCXNraW5fdG9uZRCG4/oHGhAKCWhhaXJfdG9uZRD9kL8HGgkKBGhhaXIQqAoaCAoDamF3EP8KGgkKBGJyb3cQpgwaCAoDZXllENQMGgoKBXB1cGlsEOgQGhEKCnB1cGlsX3RvbmUQtdflARoJCgRub3NlENMLGgoKBW1vdXRoEKQSGggKA2VhchCWCxoICgRib2R5EAcaEwoPZmFjZV9wcm9wb3J0aW9uEAEaDAoHZXllbGFzaBDpERoRCg1jbG90aGluZ190eXBlEAEaGgoTaGFpcl90cmVhdG1lbnRfdG9uZRCl8uwCGhIKDmVhcnJpbmdSX2xvYmUxEAYaGwoUZWFycmluZ1JfbG9iZTFfdG9uZTEQhZj/BxobChRlYXJyaW5nUl9sb2JlMV90b25lMhCFmP8HGhsKFGVhcnJpbmdSX2xvYmUxX3RvbmUzEIWY/wcaGwoUZWFycmluZ1JfbG9iZTFfdG9uZTQQhZj/BxoSCg5lYXJyaW5nTF9sb2JlMRAGGhsKFGVhcnJpbmdMX2xvYmUxX3RvbmUxEIWY/wcaGwoUZWFycmluZ0xfbG9iZTFfdG9uZTIQhZj/BxobChRlYXJyaW5nTF9sb2JlMV90b25lMxCFmP8HGhsKFGVhcnJpbmdMX2xvYmUxX3RvbmU0EIWY/wcaEgoNY2hlZWtfZGV0YWlscxD0DBoICgNoYXQQ10MaEAoJaGF0X3RvbmUxELPmzAEaEAoJaGF0X3RvbmUyELPmzAEaEAoJaGF0X3RvbmUzELPmzAEaEAoJaGF0X3RvbmU0ELPmzAEaEAoJaGF0X3RvbmU1ELPmzAEaEAoJaGF0X3RvbmU2ELPmzAEaEAoJaGF0X3RvbmU3ELPmzAEaEAoJaGF0X3RvbmU4ELPmzAEaEAoJaGF0X3RvbmU5ELPmzAEaCAoDdG9wEP4BGgsKBmJvdHRvbRC0BxoNCghmb290d2VhchD1ARoJCgRzb2NrEKcCGhAKCXRvcF90b25lMRDzwfcFGhAKCXRvcF90b25lMhDzwfcFGhAKCXRvcF90b25lMxDzwfcFGhAKCXRvcF90b25lNBDzwfcFGhAKCXRvcF90b25lNRD54fsGGhAKCXRvcF90b25lNhCjx44FGhAKCXRvcF90b25lNxD877sHGhAKCXRvcF90b25lOBDmj78EGhAKCXRvcF90b25lORDA8MwBGhEKCnRvcF90b25lMTAQgPPJAxoTCgxib3R0b21fdG9uZTEQjZuuBBoTCgxib3R0b21fdG9uZTIQjZuuBBoTCgxib3R0b21fdG9uZTMQjZuuBBoTCgxib3R0b21fdG9uZTQQjZuuBBoTCgxib3R0b21fdG9uZTUQ3LjlAhoTCgxib3R0b21fdG9uZTYQo8eOBRoTCgxib3R0b21fdG9uZTcQw4ePBhoTCgxib3R0b21fdG9uZTgQ3LTJAhoTCgxib3R0b21fdG9uZTkQnsCEARoUCg1ib3R0b21fdG9uZTEwEMWLkwYaFAoOZm9vdHdlYXJfdG9uZTEQnLRsGhQKDmZvb3R3ZWFyX3RvbmUyEJWoUBoUCg5mb290d2Vhcl90b25lMxCctGwaFAoOZm9vdHdlYXJfdG9uZTQQnLRsGhUKDmZvb3R3ZWFyX3RvbmU1EKjOoAEaFQoOZm9vdHdlYXJfdG9uZTYQ7dStAxoVCg5mb290d2Vhcl90b25lNxCozqABGhUKDmZvb3R3ZWFyX3RvbmU4EKHChAEaFQoOZm9vdHdlYXJfdG9uZTkQocKEARoVCg9mb290d2Vhcl90b25lMTAQlKBIGhEKCnNvY2tfdG9uZTEQ6+HPBxoRCgpzb2NrX3RvbmUyENOI4QEaEQoKc29ja190b25lMxDr4c8HGhEKCnNvY2tfdG9uZTQQ6+HPBxoNCglpc190dWNrZWQQAQ==");
        await Send(CreateAvatarData_Endpoint, content);
        var Update3DProfile_parameters = new Dictionary<string, string>();
        Update3DProfile_parameters.Add("background_id", "383755736");
        Update3DProfile_parameters.Add("scene_id", "383755736");
        await Send(Update3DProfile_Endpoint, Update3DProfile_parameters);
    }
    public async Task White_BlondeHaired_FemaleMoji3()
    {
        using var content = CreateStream("CP///////////wEStAsIAhAFGhAKCXNraW5fdG9uZRCG4/oHGhAKCWhhaXJfdG9uZRD9kL8HGgkKBGhhaXIQqAoaCAoDamF3EP8KGgkKBGJyb3cQpgwaCAoDZXllENQMGgoKBXB1cGlsEOgQGhEKCnB1cGlsX3RvbmUQtdflARoJCgRub3NlENMLGgoKBW1vdXRoEKQSGggKA2VhchCWCxoICgRib2R5EAcaEwoPZmFjZV9wcm9wb3J0aW9uEAEaDAoHZXllbGFzaBDpERoRCg1jbG90aGluZ190eXBlEAEaGgoTaGFpcl90cmVhdG1lbnRfdG9uZRCl8uwCGhIKDmVhcnJpbmdSX2xvYmUxEAYaGwoUZWFycmluZ1JfbG9iZTFfdG9uZTEQhZj/BxobChRlYXJyaW5nUl9sb2JlMV90b25lMhCFmP8HGhsKFGVhcnJpbmdSX2xvYmUxX3RvbmUzEIWY/wcaGwoUZWFycmluZ1JfbG9iZTFfdG9uZTQQhZj/BxoSCg5lYXJyaW5nTF9sb2JlMRAGGhsKFGVhcnJpbmdMX2xvYmUxX3RvbmUxEIWY/wcaGwoUZWFycmluZ0xfbG9iZTFfdG9uZTIQhZj/BxobChRlYXJyaW5nTF9sb2JlMV90b25lMxCFmP8HGhsKFGVhcnJpbmdMX2xvYmUxX3RvbmU0EIWY/wcaEgoNY2hlZWtfZGV0YWlscxD0DBoICgNoYXQQ10MaEAoJaGF0X3RvbmUxELPmzAEaEAoJaGF0X3RvbmUyELPmzAEaEAoJaGF0X3RvbmUzELPmzAEaEAoJaGF0X3RvbmU0ELPmzAEaEAoJaGF0X3RvbmU1ELPmzAEaEAoJaGF0X3RvbmU2ELPmzAEaEAoJaGF0X3RvbmU3ELPmzAEaEAoJaGF0X3RvbmU4ELPmzAEaEAoJaGF0X3RvbmU5ELPmzAEaCAoDdG9wEP4BGgsKBmJvdHRvbRC0BxoNCghmb290d2VhchD1ARoJCgRzb2NrEKcCGhAKCXRvcF90b25lMRDzwfcFGhAKCXRvcF90b25lMhDzwfcFGhAKCXRvcF90b25lMxDzwfcFGhAKCXRvcF90b25lNBDzwfcFGhAKCXRvcF90b25lNRD54fsGGhAKCXRvcF90b25lNhCjx44FGhAKCXRvcF90b25lNxD877sHGhAKCXRvcF90b25lOBDmj78EGhAKCXRvcF90b25lORDA8MwBGhEKCnRvcF90b25lMTAQgPPJAxoTCgxib3R0b21fdG9uZTEQjZuuBBoTCgxib3R0b21fdG9uZTIQjZuuBBoTCgxib3R0b21fdG9uZTMQjZuuBBoTCgxib3R0b21fdG9uZTQQjZuuBBoTCgxib3R0b21fdG9uZTUQ3LjlAhoTCgxib3R0b21fdG9uZTYQo8eOBRoTCgxib3R0b21fdG9uZTcQw4ePBhoTCgxib3R0b21fdG9uZTgQ3LTJAhoTCgxib3R0b21fdG9uZTkQnsCEARoUCg1ib3R0b21fdG9uZTEwEMWLkwYaFAoOZm9vdHdlYXJfdG9uZTEQnLRsGhQKDmZvb3R3ZWFyX3RvbmUyEJWoUBoUCg5mb290d2Vhcl90b25lMxCctGwaFAoOZm9vdHdlYXJfdG9uZTQQnLRsGhUKDmZvb3R3ZWFyX3RvbmU1EKjOoAEaFQoOZm9vdHdlYXJfdG9uZTYQ7dStAxoVCg5mb290d2Vhcl90b25lNxCozqABGhUKDmZvb3R3ZWFyX3RvbmU4EKHChAEaFQoOZm9vdHdlYXJfdG9uZTkQocKEARoVCg9mb290d2Vhcl90b25lMTAQlKBIGhEKCnNvY2tfdG9uZTEQ6+HPBxoRCgpzb2NrX3RvbmUyENOI4QEaEQoKc29ja190b25lMxDr4c8HGhEKCnNvY2tfdG9uZTQQ6+HPBxoNCglpc190dWNrZWQQAQ==");
        await Send(CreateAvatarData_Endpoint, content);
        var Update3DProfile_parameters = new Dictionary<string, string>();
        Update3DProfile_parameters.Add("background_id", "319087826");
        Update3DProfile_parameters.Add("scene_id", "708455430");
        await Send(Update3DProfile_Endpoint, Update3DProfile_parameters);
    }
    public async Task White_BlondeHaired_FemaleMoji4()
    {
        using var content = CreateStream("CP///////////wEStAsIAhAFGhAKCXNraW5fdG9uZRCG4/oHGhAKCWhhaXJfdG9uZRD9kL8HGgkKBGhhaXIQqAoaCAoDamF3EP8KGgkKBGJyb3cQpgwaCAoDZXllENQMGgoKBXB1cGlsEOgQGhEKCnB1cGlsX3RvbmUQtdflARoJCgRub3NlENMLGgoKBW1vdXRoEKQSGggKA2VhchCWCxoICgRib2R5EAcaEwoPZmFjZV9wcm9wb3J0aW9uEAEaDAoHZXllbGFzaBDpERoRCg1jbG90aGluZ190eXBlEAEaGgoTaGFpcl90cmVhdG1lbnRfdG9uZRCl8uwCGhIKDmVhcnJpbmdSX2xvYmUxEAYaGwoUZWFycmluZ1JfbG9iZTFfdG9uZTEQhZj/BxobChRlYXJyaW5nUl9sb2JlMV90b25lMhCFmP8HGhsKFGVhcnJpbmdSX2xvYmUxX3RvbmUzEIWY/wcaGwoUZWFycmluZ1JfbG9iZTFfdG9uZTQQhZj/BxoSCg5lYXJyaW5nTF9sb2JlMRAGGhsKFGVhcnJpbmdMX2xvYmUxX3RvbmUxEIWY/wcaGwoUZWFycmluZ0xfbG9iZTFfdG9uZTIQhZj/BxobChRlYXJyaW5nTF9sb2JlMV90b25lMxCFmP8HGhsKFGVhcnJpbmdMX2xvYmUxX3RvbmU0EIWY/wcaEgoNY2hlZWtfZGV0YWlscxD0DBoICgNoYXQQ10MaEAoJaGF0X3RvbmUxELPmzAEaEAoJaGF0X3RvbmUyELPmzAEaEAoJaGF0X3RvbmUzELPmzAEaEAoJaGF0X3RvbmU0ELPmzAEaEAoJaGF0X3RvbmU1ELPmzAEaEAoJaGF0X3RvbmU2ELPmzAEaEAoJaGF0X3RvbmU3ELPmzAEaEAoJaGF0X3RvbmU4ELPmzAEaEAoJaGF0X3RvbmU5ELPmzAEaCAoDdG9wEP4BGgsKBmJvdHRvbRC0BxoNCghmb290d2VhchD1ARoJCgRzb2NrEKcCGhAKCXRvcF90b25lMRDzwfcFGhAKCXRvcF90b25lMhDzwfcFGhAKCXRvcF90b25lMxDzwfcFGhAKCXRvcF90b25lNBDzwfcFGhAKCXRvcF90b25lNRD54fsGGhAKCXRvcF90b25lNhCjx44FGhAKCXRvcF90b25lNxD877sHGhAKCXRvcF90b25lOBDmj78EGhAKCXRvcF90b25lORDA8MwBGhEKCnRvcF90b25lMTAQgPPJAxoTCgxib3R0b21fdG9uZTEQjZuuBBoTCgxib3R0b21fdG9uZTIQjZuuBBoTCgxib3R0b21fdG9uZTMQjZuuBBoTCgxib3R0b21fdG9uZTQQjZuuBBoTCgxib3R0b21fdG9uZTUQ3LjlAhoTCgxib3R0b21fdG9uZTYQo8eOBRoTCgxib3R0b21fdG9uZTcQw4ePBhoTCgxib3R0b21fdG9uZTgQ3LTJAhoTCgxib3R0b21fdG9uZTkQnsCEARoUCg1ib3R0b21fdG9uZTEwEMWLkwYaFAoOZm9vdHdlYXJfdG9uZTEQnLRsGhQKDmZvb3R3ZWFyX3RvbmUyEJWoUBoUCg5mb290d2Vhcl90b25lMxCctGwaFAoOZm9vdHdlYXJfdG9uZTQQnLRsGhUKDmZvb3R3ZWFyX3RvbmU1EKjOoAEaFQoOZm9vdHdlYXJfdG9uZTYQ7dStAxoVCg5mb290d2Vhcl90b25lNxCozqABGhUKDmZvb3R3ZWFyX3RvbmU4EKHChAEaFQoOZm9vdHdlYXJfdG9uZTkQocKEARoVCg9mb290d2Vhcl90b25lMTAQlKBIGhEKCnNvY2tfdG9uZTEQ6+HPBxoRCgpzb2NrX3RvbmUyENOI4QEaEQoKc29ja190b25lMxDr4c8HGhEKCnNvY2tfdG9uZTQQ6+HPBxoNCglpc190dWNrZWQQAQ==");
        await Send(CreateAvatarData_Endpoint, content);
        var Update3DProfile_parameters = new Dictionary<string, string>();
        Update3DProfile_parameters.Add("background_id", "290927685");
        Update3DProfile_parameters.Add("scene_id", "582513516");
        await Send(Update3DProfile_Endpoint, Update3DProfile_parameters);
    }
}