document.addEventListener('DOMContentLoaded', function () {
    var swipers = document.querySelectorAll('.swiper');
    swipers.forEach(function (swiper) {
        new Swiper(swiper, {
            slidesPerView: 4, 
            spaceBetween: 10,
            direction: 'horizontal',
            loop: true,
            navigation: {
                nextEl: swiper.querySelector('.swiper-button-next'),
                prevEl: swiper.querySelector('.swiper-button-prev'),
            },
            breakpoints: {
                640: { slidesPerView: 1 },
                768: { slidesPerView: 2 },
                1024: { slidesPerView: 4 },
            }
        });
    });
});
