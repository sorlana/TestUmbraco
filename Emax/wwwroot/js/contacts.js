// OPTIONS
var myMap;
var myMap1;
var myMap2;
var myMap3;
var map_element = 'map';

var isMobile = false;
if(/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|ipad|iris|kindle|Android|Silk|lge |maemo|midp|mmp|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/i.test(navigator.userAgent)
    || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(navigator.userAgent.substr(0,4))) isMobile = true;

function InitMap() {
    myMap = new ymaps.Map("map", { center: [55.782729, 37.717824], zoom: 17 });
    myMap.controls.add("zoomControl").add("typeSelector").add("mapTools");

    AddPlacemark(55.782729, 37.717824);
    AddPlacemark(58.04029476730146, 56.16764850000002);
    AddPlacemark(43.112165799108894, 131.87299849999988);
}

function SetCenter(lat, lng, zoom) {
    myMap.setCenter([lng, lat], zoom);
}
function SetCenter1(lat, lng, zoom) {
    myMap1.setCenter([lng, lat], zoom);
}
function SetCenter2(lat, lng, zoom) {
    myMap2.setCenter([lng, lat], zoom);
}
function SetCenter3(lat, lng, zoom) {
    myMap3.setCenter([lng, lat], zoom);
}

function AddPlacemark(lat, lng) {
    var myPlacemark = new ymaps.Placemark([lat, lng],
        { balloonContentBody: '', }, {
        iconLayout: 'default#image',
        // ���� �� ����� ��������
        iconImageHref: '/img/location-logo.png',
        // ������� ������
        iconImageSize: [70, 50],
        // �������� �������� ���� ������������ ��������� ������
        iconImageOffset: [-35, -35]
    });
    myMap.geoObjects.add(myPlacemark);
}
function AddPlacemark1(lat, lng) {
    var myPlacemark = new ymaps.Placemark([lat, lng],
        { balloonContentBody: '', }, {
        iconLayout: 'default#image',
        // ���� �� ����� ��������
        iconImageHref: '/img/location-logo.png',
        // ������� ������
        iconImageSize: [70, 50],
        // �������� �������� ���� ������������ ��������� ������
        iconImageOffset: [-35, -35]
    });
    myMap1.geoObjects.add(myPlacemark);
}
function AddPlacemark2(lat, lng) {
    var myPlacemark = new ymaps.Placemark([lat, lng],
        { balloonContentBody: '', }, {
        iconLayout: 'default#image',
        // ���� �� ����� ��������
        iconImageHref: '/img/location-logo.png',
        // ������� ������
        iconImageSize: [70, 50],
        // �������� �������� ���� ������������ ��������� ������
        iconImageOffset: [-35, -35]
    });
    myMap2.geoObjects.add(myPlacemark);
}
function AddPlacemark3(lat, lng) {
    var myPlacemark = new ymaps.Placemark([lat, lng],
        { balloonContentBody: '', }, {
        iconLayout: 'default#image',
        // ���� �� ����� ��������
        iconImageHref: '/img/location-logo.png',
        // ������� ������
        iconImageSize: [70, 50],
        // �������� �������� ���� ������������ ��������� ������
        iconImageOffset: [-35, -35]
    });
    myMap3.geoObjects.add(myPlacemark);
}

ymaps.ready(function () {
    
    if (document.getElementById(map_element)) InitMap();
    //if (document.getElementById(map_element2)) InitMap2();
    if (document.getElementById('map1') || document.getElementById('map2') || document.getElementById('map3')) {
        myMap1 = new ymaps.Map("map1", { center: [55.782729, 37.717824], zoom: 17 });
        myMap1.controls.add("zoomControl").add("typeSelector").add("mapTools");
        //AddPlacemark1(55.782729, 37.717824);
        var myPlacemark1 = new ymaps.Placemark([55.782729, 37.717824],
            { balloonContentBody: '', }, {
            iconLayout: 'default#image',
            // ���� �� ����� ��������
            iconImageHref: '/img/location-logo.png',
            // ������� ������
            iconImageSize: [70, 50],
            // �������� �������� ���� ������������ ��������� ������
            iconImageOffset: [-35, -35]
        });
        myMap1.geoObjects.add(myPlacemark1);
        myMap2 = new ymaps.Map("map2", { center: [58.04029476730146, 56.16764850000002], zoom: 17 });
        myMap2.controls.add("zoomControl").add("typeSelector").add("mapTools");
        //AddPlacemark2(58.04029476730146, 56.16764850000002);
        var myPlacemark2 = new ymaps.Placemark([58.04029476730146, 56.16764850000002],
            { balloonContentBody: '', }, {
            iconLayout: 'default#image',
            // ���� �� ����� ��������
            iconImageHref: '/img/location-logo.png',
            // ������� ������
            iconImageSize: [70, 50],
            // �������� �������� ���� ������������ ��������� ������
            iconImageOffset: [-35, -35]
        });
        myMap2.geoObjects.add(myPlacemark2);
        myMap3 = new ymaps.Map("map3", { center: [43.112165799108894, 131.87299849999988], zoom: 17 });
        myMap3.controls.add("zoomControl").add("typeSelector").add("mapTools");
        //AddPlacemark3(43.112165799108894, 131.87299849999988);
        var myPlacemark3 = new ymaps.Placemark([43.112165799108894, 131.87299849999988],
            { balloonContentBody: '', }, {
            iconLayout: 'default#image',
            // ���� �� ����� ��������
            iconImageHref: '/img/location-logo.png',
            // ������� ������
            iconImageSize: [70, 50],
            // �������� �������� ���� ������������ ��������� ������
            iconImageOffset: [-35, -35]
        });
        myMap3.geoObjects.add(myPlacemark3);

    }
});

$(document).ready(function() {
    $('.selectOffice').click(function() {
        var tabActive = $(this).attr('data-tab');
        $("#"+tabActive).addClass('show active').siblings().removeClass('show active');
        $(this).addClass('active').siblings().removeClass('active');
        SetCenter($(this).attr('data-lat'), $(this).attr('data-lng'), 16);
    });
    $('.contact-map').click(function () {
        var tabActive = $(this).attr('data-tab');
        $("#" + tabActive).addClass('show active').siblings().removeClass('show active');
        $(this).addClass('active').siblings().removeClass('active');
        //$($(this).attr('href')).show().siblings().hide();
        SetCenter1($(this).attr('data-lat'), $(this).attr('data-lng'), 16);
        SetCenter2($(this).attr('data-lat'), $(this).attr('data-lng'), 16);
        SetCenter3($(this).attr('data-lat'), $(this).attr('data-lng'), 16);
        //return false;
    });
});