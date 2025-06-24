document.addEventListener('DOMContentLoaded', function () {
    const regionSelect = document.getElementById('region-select');
    const provinceSelect = document.getElementById('province-select');
    const districtSelect = document.getElementById('district-select');
    const wardSelect = document.getElementById('ward-select');

    // **Dữ liệu địa lý hành chính (RÚT GỌN - CHỈ ĐỂ MINH HỌA)**
    const vietnamRegionsData = {
        "Miền Bắc": {
            "Hà Nội": ["Ba Đình", "Hoàn Kiếm"],
            "Hải Phòng": ["Hồng Bàng", "Lê Chân"]
        },
        "Miền Trung": {
            "Đà Nẵng": ["Hải Châu", "Thanh Khê"],
            "Huế": ["Thành phố Huế", "Hương Thủy"]
        },
        "Miền Nam": {
            "Hồ Chí Minh": ["Quận 1", "Quận 3"],
            "Cần Thơ": ["Ninh Kiều", "Cái Răng"]
        }
    };

    const vietnamWardsData = {
        "Hà Nội": {
            "Ba Đình": ["Điện Biên", "Quán Thánh"],
            "Hoàn Kiếm": ["Lý Thái Tổ", "Tràng Tiền"]
        },
        "Hải Phòng": {
            "Hồng Bàng": ["Hoàng Văn Thụ", "Phan Bội Châu"],
            "Lê Chân": ["Cát Dài", "An Biên"]
        },
        "Đà Nẵng": {
            "Hải Châu": ["Hải Châu I", "Hải Châu II"],
            "Thanh Khê": ["Thanh Bình", "Thuận Phước"]
        },
        "Huế": {
            "Thành phố Huế": ["Vĩnh Ninh", "Phú Hội"],
            "Hương Thủy": ["Thủy Dương", "Thủy Châu"]
        },
        "Hồ Chí Minh": {
            "Quận 1": ["Bến Nghé", "Bến Thành"],
            "Quận 3": ["Phường 1", "Phường 2"]
        },
        "Cần Thơ": {
            "Ninh Kiều": ["Cái Khế", "An Hòa"],
            "Cái Răng": ["Lê Bình", "Ba Láng"]
        }
    };

    function clearDropdown(selectElement) {
        selectElement.innerHTML = '<option value="">Chọn...</option>';
        selectElement.disabled = true;
    }

    regionSelect.addEventListener('change', function () {
        const selectedRegion = this.value;
        clearDropdown(provinceSelect);
        clearDropdown(districtSelect);
        clearDropdown(wardSelect);

        if (vietnamRegionsData[selectedRegion]) {
            provinceSelect.disabled = false;
            for (const province in vietnamRegionsData[selectedRegion]) {
                const option = document.createElement('option');
                option.value = province;
                option.textContent = province;
                provinceSelect.appendChild(option);
            }
        }
    });

    provinceSelect.addEventListener('change', function () {
        const selectedRegion = regionSelect.value;
        const selectedProvince = this.value;
        clearDropdown(districtSelect);
        clearDropdown(wardSelect);

        if (vietnamRegionsData[selectedRegion] && vietnamRegionsData[selectedRegion][selectedProvince]) {
            districtSelect.disabled = false;
            vietnamRegionsData[selectedRegion][selectedProvince].forEach(district => {
                const option = document.createElement('option');
                option.value = district;
                option.textContent = district;
                districtSelect.appendChild(option);
            });
        }
    });

    districtSelect.addEventListener('change', function () {
        const selectedProvince = provinceSelect.value;
        const selectedDistrict = this.value;
        clearDropdown(wardSelect);

        if (vietnamWardsData[selectedProvince] && vietnamWardsData[selectedProvince][selectedDistrict]) {
            wardSelect.disabled = false;
            vietnamWardsData[selectedProvince][selectedDistrict].forEach(ward => {
                const option = document.createElement('option');
                option.value = ward;
                option.textContent = ward;
                wardSelect.appendChild(option);
            });
        }
    });
});