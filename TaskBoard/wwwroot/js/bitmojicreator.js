// Setup buttons

$('#btn_createBitmoji').on('click', async (e) => {
    await BlockingButtonAction(e.target, async () => {
        let Name = $('#bitmoji_name').val();
        let Scale = 1;
        let Gender = $('#gender').val();
        let Style = 5;
        let Rotation = $('#rotation').val();
        let Body = $('#body').val();
        let Bottom = $('#bottom').val();
        let BottomTone1 = $('#bottom_tone1').val();
        let BottomTone2 = $('#bottom_tone2').val();
        let BottomTone3 = $('#bottom_tone3').val();
        let BottomTone4 = $('#bottom_tone4').val();
        let BottomTone5 = $('#bottom_tone5').val();
        let BottomTone6 = $('#bottom_tone6').val();
        let BottomTone7 = $('#bottom_tone7').val();
        let BottomTone8 = $('#bottom_tone8').val();
        let BottomTone9 = $('#bottom_tone9').val();
        let BottomTone10 = $('#bottom_tone10').val();
        let Brow = $('#brow').val();
        let ClothingType = $('#clothing_type').val();
        let Ear = $('#ear').val();
        let Eye = $('#eye').val();
        let Eyelash = $('#eyelash').val();
        let FaceProportion = $('#face_proportion').val();
        let Footwear = $('#footwear').val();
        let FootwearTone1 = $('#footwear_tone1').val();
        let FootwearTone2 = $('#footwear_tone2').val();
        let FootwearTone3 = $('#footwear_tone3').val();
        let FootwearTone4 = $('#footwear_tone4').val();
        let FootwearTone5 = $('#footwear_tone5').val();
        let FootwearTone6 = $('#footwear_tone6').val();
        let FootwearTone7 = $('#footwear_tone7').val();
        let FootwearTone8 = $('#footwear_tone8').val();
        let FootwearTone9 = $('#footwear_tone9').val();
        let FootwearTone10 = $('#footwear_tone10').val();
        let Hair = $('#hair').val();
        let HairTone = $('#hair_tone').val();
        let IsTucked = $('#is_tucked').val();
        let Jaw = $('#jaw').val();
        let Mouth = $('#mouth').val();
        let Nose = $('#nose').val();
        let Pupil = $('#pupil').val();
        let PupilTone = $('#pupil_tone').val();
        let SkinTone = $('#skin_tone').val();
        let Sock = $('#sock').val();
        let SockTone1 = $('#sock_tone1').val();
        let SockTone2 = $('#sock_tone2').val();
        let SockTone3 = $('#sock_tone3').val();
        let SockTone4 = $('#sock_tone4').val();
        let Top = $('#top').val();
        let TopTone1 = $('#top_tone1').val();
        let TopTone2 = $('#top_tone2').val();
        let TopTone3 = $('#top_tone3').val();
        let TopTone4 = $('#top_tone4').val();
        let TopTone5 = $('#top_tone5').val();
        let TopTone6 = $('#top_tone6').val();
        let TopTone7 = $('#top_tone7').val();
        let TopTone8 = $('#top_tone8').val();
        let TopTone9 = $('#top_tone9').val();
        let TopTone10 = $('#top_tone10').val();
        let Version = $('#version').val();

        try {
            let args = CreateActionArguments({
                Name: Name,
                Scale : 1,
                Gender : Gender,
                Style : 5,
                Rotation : Rotation,
                Body : Body,
                Bottom : Bottom,
                BottomTone1 : BottomTone1,
                BottomTone2 : BottomTone2,
                BottomTone3 : BottomTone3,
                BottomTone4 : BottomTone4,
                BottomTone5 : BottomTone5,
                BottomTone6 : BottomTone6,
                BottomTone7 : BottomTone7,
                BottomTone8 : BottomTone8,
                BottomTone9 : BottomTone9,
                BottomTone10 : BottomTone10,
                Brow : Brow,
                ClothingType : ClothingType,
                Ear : Ear,
                Eye : Eye,
                Eyelash : Eyelash,
                FaceProportion : FaceProportion,
                Footwear : Footwear,
                FootwearTone1 : FootwearTone1,
                FootwearTone2 : FootwearTone2,
                FootwearTone3 : FootwearTone3,
                FootwearTone4 : FootwearTone4,
                FootwearTone5 : FootwearTone5,
                FootwearTone6 : FootwearTone6,
                FootwearTone7 : FootwearTone7,
                FootwearTone8 : FootwearTone8,
                FootwearTone9 : FootwearTone9,
                FootwearTone10 : FootwearTone10,
                Hair : Hair,
                HairTone : HairTone,
                IsTucked : IsTucked,
                Jaw : Jaw,
                Mouth : Mouth,
                Nose : Nose,
                Pupil : Pupil,
                PupilTone : PupilTone,
                SkinTone : SkinTone,
                Sock : Sock,
                SockTone1 : SockTone1,
                SockTone2 : SockTone2,
                SockTone3 : SockTone3,
                SockTone4 : SockTone4,
                Top : Top,
                TopTone1 : TopTone1,
                TopTone2 : TopTone2,
                TopTone3 : TopTone3,
                TopTone4 : TopTone4,
                TopTone5 : TopTone5,
                TopTone6 : TopTone6,
                TopTone7 : TopTone7,
                TopTone8 : TopTone8,
                TopTone9 : TopTone9,
                TopTone10 : TopTone10,
                Version : 0
            });
            let result = await CreateBitmoji(args);
            logger.Info(result.message);
        } catch (e) {
            logger.PrintException(e);
        }
    });
});

async function CreateBitmoji(args) {
    return await api.CreateBitmoji(args);
}

let alertManager = new AlertManager();
let logger = new Logger('#messages', alertManager);
let api = new Api(logger);