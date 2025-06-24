// Trong file js/main.js
var fullHeight = function () {
	// ... (các code khác)
};
fullHeight();

var heroSlider = function () {
	$('.home-slider').owlCarousel({
		loop: true,
		autoplay: true,
		margin: 0,
		animateOut: 'fadeOut', // Hoặc hiệu ứng khác
		animateIn: 'fadeIn',   // Hoặc hiệu ứng khác
		nav: true, // true để hiển thị nút next/prev
		autoplayHoverPause: false,
		items: 1,
		navText: ["<span class='ion-md-arrow-back'></span>", "<span class='ion-chevron-right'></span>"],
		responsive: {
			0: {
				items: 1,
				nav: false // Ẩn nút nav trên mobile nếu muốn
			},
			600: {
				items: 1,
				nav: false
			},
			1000: {
				items: 1,
				nav: true
			}
		}
	});
};
heroSlider(); // Gọi hàm để khởi tạo
AOS.init({
 	duration: 800,
 	easing: 'slide'
 });
//---------------------------------------------
document.addEventListener("DOMContentLoaded", function () {
	const fruitIcons = ["🍎", "🍌", "🍇", "🍉", "🍒", "🍍", "🥑", "🍊"];

	document.addEventListener("click", function (event) {
		createFruitEffect(event.pageX, event.pageY);
	});

	function createFruitEffect(x, y) {
		const fruit = document.createElement("span");
		fruit.innerText = fruitIcons[Math.floor(Math.random() * fruitIcons.length)];
		fruit.classList.add("fruit-effect");

		fruit.style.left = `${x}px`;
		fruit.style.top = `${y}px`;

		document.body.appendChild(fruit);

		// Tạo hiệu ứng bay lên & biến mất
		setTimeout(() => fruit.remove(), 1000);
	}
});
//-----------------------------------------------thồng kê

function updateFields() {
	var filterType = document.getElementById("filterType").value;
	document.getElementById("year").style.display = "block";
	document.getElementById("month").style.display = (filterType === "day" || filterType === "month") ? "block" : "none";
	document.getElementById("day").style.display = (filterType === "day") ? "block" : "none";

	// Lưu lại giá trị filterType vào localStorage
	localStorage.setItem("filterType", filterType);

	// Gọi hàm cập nhật thống kê
	updateRevenueStatistics();
}

function setDefaultDate() {
	var today = new Date();
	var yearInput = document.getElementById("year");
	var monthInput = document.getElementById("month");
	var dayInput = document.getElementById("day");

	// Nếu giá trị từ Model không có, đặt giá trị mặc định là ngày hiện tại
	if (!yearInput.value) {
		yearInput.value = today.getFullYear();
	}
	if (!monthInput.value) {
		monthInput.value = today.getMonth() + 1; // Tháng trong JS bắt đầu từ 0
	}
	if (!dayInput.value) {
		dayInput.value = today.getDate();
	}

	// Kiểm tra và thiết lập giá trị đã lưu từ localStorage
	if (localStorage.getItem("year")) {
		yearInput.value = localStorage.getItem("year");
	}
	if (localStorage.getItem("month")) {
		monthInput.value = localStorage.getItem("month");
	}
	if (localStorage.getItem("day")) {
		dayInput.value = localStorage.getItem("day");
	}

	// Nếu có filterType đã lưu trong localStorage, áp dụng nó
	var savedFilterType = localStorage.getItem("filterType");
	if (savedFilterType) {
		document.getElementById("filterType").value = savedFilterType;
		updateFields(); // Cập nhật hiển thị các trường
	}
}

function saveSelectedDate() {
	// Lưu lại giá trị của ngày, tháng, năm vào localStorage khi người dùng thay đổi
	var year = document.getElementById("year").value;
	var month = document.getElementById("month").value;
	var day = document.getElementById("day").value;

	localStorage.setItem("year", year);
	localStorage.setItem("month", month);
	localStorage.setItem("day", day);

	// Gọi lại hàm cập nhật thống kê
	updateRevenueStatistics();
}

function updateRevenueStatistics() {
	var filterType = document.getElementById("filterType").value;
	var year = document.getElementById("year").value;
	var month = document.getElementById("month").value;
	var day = document.getElementById("day").value;

	// Sử dụng Fetch API (hoặc AJAX) để gửi yêu cầu tới Controller và nhận kết quả
	var url = '/Admin/RevenueStatistics?filterType=' + filterType + '&year=' + year + '&month=' + month + '&day=' + day;

	fetch(url)
		.then(response => response.json())
		.then(data => {
			// Cập nhật thông tin doanh thu trên trang mà không cần tải lại trang
			document.getElementById('revenue').innerText = 'Doanh thu: ' + data.TotalRevenue;
			document.getElementById('orderCount').innerText = 'Số đơn hàng: ' + data.OrderCount;
		})
		.catch(error => {
			console.error('Error:', error);
		});
}

window.onload = function () {
	setDefaultDate(); // Thiết lập ngày mặc định từ localStorage

	// Nếu đã có lựa chọn từ localStorage, áp dụng nó
	var savedFilterType = localStorage.getItem("filterType");
	if (savedFilterType) {
		document.getElementById("filterType").value = savedFilterType;
		updateFields(); // Cập nhật các trường ngày/tháng/năm
	}

	// Gắn sự kiện để lưu lại giá trị khi người dùng thay đổi ngày tháng
	document.getElementById("year").addEventListener("change", saveSelectedDate);
	document.getElementById("month").addEventListener("change", saveSelectedDate);
	document.getElementById("day").addEventListener("change", saveSelectedDate);
	document.getElementById("filterType").addEventListener("change", updateFields);
};



//----------------------------------------------cart
$(document).ready(function () {
	// Sự kiện khi nhấn nút "+"
	$(".plus-btn").on("click", function () {
		var cartId = $(this).data("id");
		var quantity = parseInt($(".cart-quantity[data-id='" + cartId + "']").val()) + 1;
		updateCartQuantity(cartId, quantity);
	});

	// Sự kiện khi nhấn nút "-"
	$(".minus-btn").on("click", function () {
		var cartId = $(this).data("id");
		var quantity = parseInt($(".cart-quantity[data-id='" + cartId + "']").val()) - 1;
		if (quantity > 0) {
			updateCartQuantity(cartId, quantity);
		}
	});

	// Hàm cập nhật số lượng sản phẩm trong giỏ hàng
	function updateCartQuantity(cartId, quantity) {
		$.ajax({
			url: '/BanHang/UpdateCart',  // Đường dẫn đến phương thức controller để xử lý cập nhật giỏ hàng
			type: 'POST',
			data: {
				id: cartId,
				quantity: quantity
			},
			success: function (response) {
				if (response.success) {
					// Cập nhật lại số lượng và tổng tiền trong giao diện
					$(".cart-quantity[data-id='" + cartId + "']").val(quantity);
					$(".total-price-" + cartId).text(response.newTotalPrice); // Cập nhật tổng tiền
				} else {
					alert("Số lượng hàng tồn kho không đủ \nXin quý khách thông cảm.");
				}
			},
			error: function () {
				alert("Lỗi khi cập nhật giỏ hàng.");
			}
		});
	}
});

//----------------------------------------------

$(document).ready(function () {
	$('.addToCartButton').click(function () {
		var productId = $(this).data('product-id'); // Lấy productId từ data-product-id
		var quantity = 1; // Mặc định số lượng là 1

		if (!productId) {
			console.error("Không tìm thấy productId!");
			return;
		}

		var productItem = $(this).closest('.product-item');
		var productImage = productItem.find('.img-fluid');
		var cartDisplay = $('.icon-shopping_cart').parent();

		// Hiệu ứng bay vào giỏ hàng
		var productImageClone = productImage.clone().css({
			position: 'absolute',
			top: productImage.offset().top,
			left: productImage.offset().left,
			width: productImage.width(),
			height: productImage.height(),
			zIndex: 9999
		}).appendTo('body');

		productImageClone.animate({
			top: cartDisplay.offset().top,
			left: cartDisplay.offset().left,
			width: 25,
			height: 25
		}, 1000, function () {
			productImageClone.remove();

			// Cập nhật số lượng giỏ hàng hiển thị trên icon
			var currentCount = parseInt(cartDisplay.text().match(/\[(\d+)\]/)?.[1] || 0);
			cartDisplay.html('<span class="icon-shopping_cart"></span> [' + (currentCount + quantity) + ']');
		});

		// Gửi AJAX request để thêm sản phẩm vào giỏ hàng mà không có thông báo
		$.ajax({
			url: '/BanHang/AddToCart',
			type: 'POST',
			data: { productId: productId, quantity: quantity },
			success: function (response) {
				// Không làm gì cả khi thành công
			},
			error: function (error) {
				console.error('Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng.', error);
			}
		});
	});
});



 //---------------------------------------------


(function($) {

	"use strict";

	var isMobile = {
		Android: function() {
			return navigator.userAgent.match(/Android/i);
		},
			BlackBerry: function() {
			return navigator.userAgent.match(/BlackBerry/i);
		},
			iOS: function() {
			return navigator.userAgent.match(/iPhone|iPad|iPod/i);
		},
			Opera: function() {
			return navigator.userAgent.match(/Opera Mini/i);
		},
			Windows: function() {
			return navigator.userAgent.match(/IEMobile/i);
		},
			any: function() {
			return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
		}
	};


	$(window).stellar({
    responsive: true,
    parallaxBackgrounds: true,
    parallaxElements: true,
    horizontalScrolling: false,
    hideDistantElements: false,
    scrollProperty: 'scroll'
  });


	var fullHeight = function() {

		$('.js-fullheight').css('height', $(window).height());
		$(window).resize(function(){
			$('.js-fullheight').css('height', $(window).height());
		});

	};
	fullHeight();

	// loader
	var loader = function() {
		setTimeout(function() { 
			if($('#ftco-loader').length > 0) {
				$('#ftco-loader').removeClass('show');
			}
		}, 1);
	};
	loader();

	// Scrollax
   $.Scrollax();

	var carousel = function() {
		$('.home-slider').owlCarousel({
	    loop:true,
	    autoplay: true,
	    margin:0,
	    animateOut: 'fadeOut',
	    animateIn: 'fadeIn',
	    nav:false,
	    autoplayHoverPause: false,
	    items: 1,
	    navText : ["<span class='ion-md-arrow-back'></span>","<span class='ion-chevron-right'></span>"],
	    responsive:{
	      0:{
	        items:1
	      },
	      600:{
	        items:1
	      },
	      1000:{
	        items:1
	      }
	    }
		});
	
		$('.carousel-testimony').owlCarousel({
			center: true,
			loop: true,
			items:1,
			margin: 30,
			stagePadding: 0,
			nav: false,
			navText: ['<span class="ion-ios-arrow-back">', '<span class="ion-ios-arrow-forward">'],
			responsive:{
				0:{
					items: 1
				},
				600:{
					items: 3
				},
				1000:{
					items: 3
				}
			}
		});

	};
	carousel();

	$('nav .dropdown').hover(function(){
    var $this = $(this);
    $this.addClass('show');
    $this.find('> a').attr('aria-expanded', true);
    $this.find('.dropdown-menu').addClass('show');
}, function(){
    var $this = $(this);
    setTimeout(function(){ // Thêm setTimeout để tạo độ trễ
        $this.removeClass('show');
        $this.find('> a').attr('aria-expanded', false);
        $this.find('.dropdown-menu').removeClass('show');
    }, 600); // 300ms (điều chỉnh thời gian tại đây)
});


	$('#dropdown04').on('show.bs.dropdown', function () {
	  console.log('show');
	});

	// scroll
	var scrollWindow = function() {
		$(window).scroll(function(){
			var $w = $(this),
					st = $w.scrollTop(),
					navbar = $('.ftco_navbar'),
					sd = $('.js-scroll-wrap');

			if (st > 150) {
				if ( !navbar.hasClass('scrolled') ) {
					navbar.addClass('scrolled');	
				}
			} 
			if (st < 150) {
				if ( navbar.hasClass('scrolled') ) {
					navbar.removeClass('scrolled sleep');
				}
			} 
			if ( st > 350 ) {
				if ( !navbar.hasClass('awake') ) {
					navbar.addClass('awake');	
				}
				
				if(sd.length > 0) {
					sd.addClass('sleep');
				}
			}
			if ( st < 350 ) {
				if ( navbar.hasClass('awake') ) {
					navbar.removeClass('awake');
					navbar.addClass('sleep');
				}
				if(sd.length > 0) {
					sd.removeClass('sleep');
				}
			}
		});
	};
	scrollWindow();

	
	var counter = function() {
		
		$('#section-counter').waypoint( function( direction ) {

			if( direction === 'down' && !$(this.element).hasClass('ftco-animated') ) {

				var comma_separator_number_step = $.animateNumber.numberStepFactories.separator(',')
				$('.number').each(function(){
					var $this = $(this),
						num = $this.data('number');
						console.log(num);
					$this.animateNumber(
					  {
					    number: num,
					    numberStep: comma_separator_number_step
					  }, 7000
					);
				});
				
			}

		} , { offset: '95%' } );

	}
	counter();

	var contentWayPoint = function() {
		var i = 0;
		$('.ftco-animate').waypoint( function( direction ) {

			if( direction === 'down' && !$(this.element).hasClass('ftco-animated') ) {
				
				i++;

				$(this.element).addClass('item-animate');
				setTimeout(function(){

					$('body .ftco-animate.item-animate').each(function(k){
						var el = $(this);
						setTimeout( function () {
							var effect = el.data('animate-effect');
							if ( effect === 'fadeIn') {
								el.addClass('fadeIn ftco-animated');
							} else if ( effect === 'fadeInLeft') {
								el.addClass('fadeInLeft ftco-animated');
							} else if ( effect === 'fadeInRight') {
								el.addClass('fadeInRight ftco-animated');
							} else {
								el.addClass('fadeInUp ftco-animated');
							}
							el.removeClass('item-animate');
						},  k * 50, 'easeInOutExpo' );
					});
					
				}, 100);
				
			}

		} , { offset: '95%' } );
	};
	contentWayPoint();


	// navigation
	var OnePageNav = function() {
		$(".smoothscroll[href^='#'], #ftco-nav ul li a[href^='#']").on('click', function(e) {
		 	e.preventDefault();

		 	var hash = this.hash,
		 			navToggler = $('.navbar-toggler');
		 	$('html, body').animate({
		    scrollTop: $(hash).offset().top
		  }, 700, 'easeInOutExpo', function(){
		    window.location.hash = hash;
		  });


		  if ( navToggler.is(':visible') ) {
		  	navToggler.click();
		  }
		});
		$('body').on('activate.bs.scrollspy', function () {
		  console.log('nice');
		})
	};
	OnePageNav();


	// magnific popup
	$('.image-popup').magnificPopup({
    type: 'image',
    closeOnContentClick: true,
    closeBtnInside: false,
    fixedContentPos: true,
    mainClass: 'mfp-no-margins mfp-with-zoom', // class to remove default margin from left and right side
     gallery: {
      enabled: true,
      navigateByImgClick: true,
      preload: [0,1] // Will preload 0 - before current, and 1 after the current image
    },
    image: {
      verticalFit: true
    },
    zoom: {
      enabled: true,
      duration: 300 // don't foget to change the duration also in CSS
    }
  });

  $('.popup-youtube, .popup-vimeo, .popup-gmaps').magnificPopup({
    disableOn: 700,
    type: 'iframe',
    mainClass: 'mfp-fade',
    removalDelay: 160,
    preloader: false,

    fixedContentPos: false
  });



	var goHere = function() {

		$('.mouse-icon').on('click', function(event){
			
			event.preventDefault();

			$('html,body').animate({
				scrollTop: $('.goto-here').offset().top
			}, 500, 'easeInOutExpo');
			
			return false;
		});
	};
	goHere();


	function makeTimer() {

		var endTime = new Date("21 December 2019 9:56:00 GMT+01:00");			
		endTime = (Date.parse(endTime) / 1000);

		var now = new Date();
		now = (Date.parse(now) / 1000);

		var timeLeft = endTime - now;

		var days = Math.floor(timeLeft / 86400); 
		var hours = Math.floor((timeLeft - (days * 86400)) / 3600);
		var minutes = Math.floor((timeLeft - (days * 86400) - (hours * 3600 )) / 60);
		var seconds = Math.floor((timeLeft - (days * 86400) - (hours * 3600) - (minutes * 60)));

		if (hours < "10") { hours = "0" + hours; }
		if (minutes < "10") { minutes = "0" + minutes; }
		if (seconds < "10") { seconds = "0" + seconds; }

		$("#days").html(days + "<span>Days</span>");
		$("#hours").html(hours + "<span>Hours</span>");
		$("#minutes").html(minutes + "<span>Minutes</span>");
		$("#seconds").html(seconds + "<span>Seconds</span>");		

}

setInterval(function() { makeTimer(); }, 1000);



})(jQuery);



//-------------------------------------------------------------------------
(function ($) {
	"use strict";

	// ... (các phần mã khác trong main.js) ...

	$(document).ready(function () {
		$(".addToCartButton").click(function (e) {
			e.preventDefault();

			var productId = $(this).data("product-id");

			$.ajax({
				url: "/Cart/AddToCart",
				type: "POST",
				data: {
					productId: productId,
					quantity: 1,
					__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
				},
				success: function (response) {
					if (response.success) {

						// Cập nhật số lượng giỏ hàng ngay lập tức
						$("#cart-count").text(response.totalItems);

						// Cập nhật số lượng giỏ hàng hiển thị trên icon (nếu có)
						var cartDisplay = $('.icon-shopping_cart').parent();
						if (cartDisplay.length) {
							cartDisplay.html('<span class="icon-shopping_cart"></span> [' + response.totalItems + ']');
						}

						// Cập nhật bảng giỏ hàng (nếu có)
						if ($("#cart-table tbody").length && response.cartItem) {
							var cartItem = response.cartItem;
							var newRow = "<tr>" +
								"<td>" + cartItem.productName + "</td>" +
								"<td>" + cartItem.quantity + "</td>" +
								"<td>" + cartItem.price + "</td>" +
								"</tr>";
							$("#cart-table tbody").append(newRow);
						}
					} else {
					}
				},
			
			});
		});
	});

	// ... (các phần mã khác trong main.js) ...

})(jQuery);
//--------------------------------------------------------------------------
	document.addEventListener('DOMContentLoaded', function () {
        const favoriteButtons = document.querySelectorAll('.add-to-favorite');

        favoriteButtons.forEach(button => {
		button.addEventListener('click', function (e) {
			e.preventDefault();
			const productId = this.getAttribute('data-product-id');

			const formData = new FormData();
			formData.append('productId', productId);

			// Lấy token từ input ẩn
			const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
			if (token) {
				formData.append('__RequestVerificationToken', token);
			}

			fetch('/Favorite/Add', {
				method: 'POST',
				body: formData
			})
				.then(response => {
					if (response.ok) {
						console.log("Đã thêm vào yêu thích!");
						// Thêm hiệu ứng nếu muốn, ví dụ: đổi màu icon
						button.querySelector('i').classList.add('text-danger');
					} else {
						console.error("Lỗi khi thêm vào yêu thích!");
					}
				});
		});
        });
    });

	document.addEventListener('DOMContentLoaded', function () {
        const alertBox = document.getElementById('favorite-alert');

        document.querySelectorAll('.remove-favorite-btn').forEach(button => {
		button.addEventListener('click', function () {
			const favoriteId = this.getAttribute('data-id');
			const row = this.closest('tr');

			const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
			if (!token) return alert("Thiếu CSRF token!");

			fetch(`/Favorite/Remove/${favoriteId}`, {
				method: 'POST',
				headers: {
					'RequestVerificationToken': token
				}
			})
				.then(response => {
					if (response.ok) {
						row.remove(); // Xoá dòng khỏi bảng

						alertBox.textContent = "Đã xoá khỏi danh sách yêu thích!";
						alertBox.classList.remove('d-none');
						alertBox.classList.add('alert-success');

						// Ẩn sau 3 giây
						setTimeout(() => {
							alertBox.classList.add('d-none');
						}, 3000);
					} else {
						alert("Xoá thất bại.");
					}
				});
		});
        });
    });

	function showLogoutConfirm(event) {
		event.preventDefault();

	Swal.fire({
		title: 'Xác nhận đăng xuất',
	text: 'Bạn có chắc chắn muốn đăng xuất không?',
	icon: 'warning',
	showCancelButton: true,
	confirmButtonText: 'Đăng xuất',
	cancelButtonText: 'Hủy',
	confirmButtonColor: '#d33',
		cancelButtonColor: '#82ae46',
        }).then((result) => {
            if (result.isConfirmed) {
		document.getElementById("logoutForm").submit();
            }
        });
    }





$(document).ready(function () {
	// Định nghĩa các tỉnh theo miền
	var regions = {
		'Miền Bắc': [
			'Hà Nội', 'Bắc Giang', 'Bắc Kạn', 'Bắc Ninh', 'Cao Bằng',
			'Điện Biên', 'Hải Dương', 'Hải Phòng', 'Hòa Bình', 'Hưng Yên',
			'Lai Châu', 'Lạng Sơn', 'Lào Cai', 'Nam Định', 'Nghệ An',
			'Ninh Bình', 'Phú Thọ', 'Quảng Ninh', 'Sơn La', 'Tuyên Quang',
			'Thái Bình', 'Thái Nguyên', 'Thanh Hóa', 'Vĩnh Phúc'
		],

		'Miền Trung': [
			'Đà Nẵng', 'Bình Định', 'Gia Lai', 'Khánh Hòa', 'Kon Tum',
			'Quảng Bình', 'Quảng Nam', 'Quảng Ngãi', 'Quảng Trị', 'Thừa Thiên-Huế'
		],
		'Miền Nam': [
			'Hồ Chí Minh', 'Bà Rịa-Vũng Tàu', 'Bình Dương', 'Bình Phước',
			'Đồng Nai', 'Đồng Tháp', 'Hậu Giang', 'Long An', 'Tiền Giang',
			'Trà Vinh', 'Vĩnh Long', 'An Giang', 'Kiên Giang', 'Cần Thơ',
			'Sóc Trăng', 'Bến Tre', 'Tây Ninh', 'Vũng Tàu'
		]
	};

	// Hàm cập nhật tỉnh khi thay đổi miền
	function updateProvinces(region) {
		var provinceSelect = $('#provinceSelect');
		provinceSelect.empty(); // Xóa hết các tỉnh hiện tại

		if (regions[region]) {
			// Thêm các tỉnh mới vào
			regions[region].forEach(function (province) {
				provinceSelect.append(new Option(province, province));
			});
		}
	}

	// Gọi hàm cập nhật tỉnh khi chọn miền
	$('#regionSelect').change(function () {
		var selectedRegion = $(this).val();
		updateProvinces(selectedRegion);
	});

	// Thêm tỉnh cho miền mặc định khi trang tải
	updateProvinces($('#regionSelect').val());
});
