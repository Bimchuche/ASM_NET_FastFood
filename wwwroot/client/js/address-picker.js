/* ============================================
   LEAFLET ADDRESS PICKER
   Free OpenStreetMap-based address selection
   ============================================ */

// Initialize map and marker (use var to allow re-declaration)
var addressMap = window.addressMap || null;
var addressMarker = window.addressMarker || null;

// Initialize Leaflet map
function initAddressMap(containerId, inputId, options = {}) {
    // Check if already initialized for this container
    var container = document.getElementById(containerId);
    if (!container) {
        console.log('Map container not found:', containerId);
        return null;
    }
    
    // If map already exists, remove it first
    if (addressMap) {
        try { addressMap.remove(); } catch(e) {}
    }
    
    // Default Ho Chi Minh City location
    var defaultLat = options.lat || 10.8231;
    var defaultLng = options.lng || 106.6297;
    var defaultZoom = options.zoom || 13;

    // Create map
    addressMap = L.map(containerId).setView([defaultLat, defaultLng], defaultZoom);
    window.addressMap = addressMap;

    // Add OpenStreetMap tiles
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap'
    }).addTo(addressMap);

    // Add marker
    addressMarker = L.marker([defaultLat, defaultLng], { draggable: true }).addTo(addressMap);
    window.addressMarker = addressMarker;

    // Click on map to move marker
    addressMap.on('click', function(e) {
        addressMarker.setLatLng(e.latlng);
        reverseGeocode(e.latlng.lat, e.latlng.lng, inputId);
    });

    // Drag marker to update address
    addressMarker.on('dragend', function(e) {
        var pos = addressMarker.getLatLng();
        reverseGeocode(pos.lat, pos.lng, inputId);
    });

    // If we have existing address, try to geocode it
    var input = document.getElementById(inputId);
    if (input && input.value && input.value.trim() !== '') {
        searchAddressAndMove(input.value);
    }

    return { map: addressMap, marker: addressMarker };
}

// Update coordinate hidden inputs
function updateCoordinateInputs(lat, lng) {
    var latInput = document.getElementById('addressLatitude');
    var lngInput = document.getElementById('addressLongitude');
    if (latInput) latInput.value = lat;
    if (lngInput) lngInput.value = lng;
}

// Reverse geocode (lat/lng -> address)
function reverseGeocode(lat, lng, inputId) {
    // Update coordinate inputs
    updateCoordinateInputs(lat, lng);
    
    var url = 'https://nominatim.openstreetmap.org/reverse?format=json&lat=' + lat + '&lon=' + lng + '&accept-language=vi';
    
    fetch(url)
        .then(function(response) { return response.json(); })
        .then(function(data) {
            if (data.display_name) {
                var input = document.getElementById(inputId);
                if (input) {
                    input.value = data.display_name;
                    input.dispatchEvent(new Event('change'));
                }
            }
        })
        .catch(function(err) { console.log('Geocode error:', err); });
}

// Search address and move map (address -> lat/lng)
function searchAddressAndMove(query) {
    if (!query || query.trim() === '') return;
    
    var url = 'https://nominatim.openstreetmap.org/search?format=json&q=' + encodeURIComponent(query) + '&limit=1&accept-language=vi&countrycodes=vn';
    
    fetch(url)
        .then(function(response) { return response.json(); })
        .then(function(data) {
            if (data && data.length > 0) {
                var lat = parseFloat(data[0].lat);
                var lng = parseFloat(data[0].lon);
                
                if (window.addressMap && window.addressMarker) {
                    window.addressMap.setView([lat, lng], 16);
                    window.addressMarker.setLatLng([lat, lng]);
                }
            }
        })
        .catch(function(err) { console.log('Search error:', err); });
}

// Initialize address search with autocomplete suggestions
function initAddressAutocomplete(inputId, suggestionsId) {
    var input = document.getElementById(inputId);
    var suggestions = document.getElementById(suggestionsId);
    
    if (!input || !suggestions) return;
    
    var debounceTimer;
    
    input.addEventListener('input', function() {
        clearTimeout(debounceTimer);
        var query = this.value.trim();
        
        if (query.length < 3) {
            suggestions.style.display = 'none';
            suggestions.innerHTML = '';
            return;
        }
        
        debounceTimer = setTimeout(function() {
            searchAddressSuggestions(query, suggestionsId, inputId);
        }, 500);
    });
    
    // Also move map when user presses Enter
    input.addEventListener('keydown', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            suggestions.style.display = 'none';
            searchAddressAndMove(this.value);
        }
    });
    
    // Move map when input loses focus (blur)
    input.addEventListener('blur', function() {
        var query = this.value.trim();
        if (query.length > 5) {
            setTimeout(function() {
                searchAddressAndMove(query);
            }, 300);
        }
    });
    
    // Hide suggestions when clicking outside
    document.addEventListener('click', function(e) {
        if (!input.contains(e.target) && !suggestions.contains(e.target)) {
            suggestions.style.display = 'none';
        }
    });
}

// Get address suggestions
function searchAddressSuggestions(query, suggestionsId, inputId) {
    var suggestions = document.getElementById(suggestionsId);
    var url = 'https://nominatim.openstreetmap.org/search?format=json&q=' + encodeURIComponent(query) + '&limit=5&accept-language=vi&countrycodes=vn';
    
    fetch(url)
        .then(function(response) { return response.json(); })
        .then(function(data) {
            if (data && data.length > 0) {
                suggestions.innerHTML = '';
                data.forEach(function(item) {
                    var div = document.createElement('div');
                    div.className = 'address-suggestion';
                    div.innerHTML = '<span class="suggestion-icon">📍</span>' + item.display_name;
                    div.onclick = function() {
                        document.getElementById(inputId).value = item.display_name;
                        suggestions.style.display = 'none';
                        
                        var lat = parseFloat(item.lat);
                        var lng = parseFloat(item.lon);
                        
                        // Update coordinate inputs
                        updateCoordinateInputs(lat, lng);
                        
                        // Move map
                        if (window.addressMap && window.addressMarker) {
                            window.addressMap.setView([lat, lng], 16);
                            window.addressMarker.setLatLng([lat, lng]);
                        }
                    };
                    suggestions.appendChild(div);
                });
                suggestions.style.display = 'block';
            } else {
                suggestions.style.display = 'none';
            }
        })
        .catch(function(err) {
            console.log('Suggestions error:', err);
            suggestions.style.display = 'none';
        });
}

// Get current location
function getCurrentLocation(inputId) {
    if (!navigator.geolocation) {
        console.log('Geolocation not supported');
        return;
    }
    
    // Show loading indicator
    var btn = event.target;
    var originalText = btn.textContent;
    btn.textContent = '⏳ Đang tìm...';
    btn.disabled = true;
    
    navigator.geolocation.getCurrentPosition(
        function(position) {
            var lat = position.coords.latitude;
            var lng = position.coords.longitude;
            
            if (window.addressMap && window.addressMarker) {
                window.addressMap.setView([lat, lng], 16);
                window.addressMarker.setLatLng([lat, lng]);
            }
            
            reverseGeocode(lat, lng, inputId);
            
            // Restore button
            btn.textContent = originalText;
            btn.disabled = false;
        },
        function(error) {
            // Restore button
            btn.textContent = originalText;
            btn.disabled = false;
            
            // Friendly error messages (no alert)
            var msg = '';
            switch(error.code) {
                case error.PERMISSION_DENIED:
                    msg = 'Bạn chưa cho phép truy cập vị trí. Hãy gõ địa chỉ hoặc click trên bản đồ.';
                    break;
                case error.POSITION_UNAVAILABLE:
                    msg = 'Không thể xác định vị trí. Hãy gõ địa chỉ hoặc click trên bản đồ.';
                    break;
                case error.TIMEOUT:
                    msg = 'Hết thời gian chờ. Hãy thử lại hoặc gõ địa chỉ.';
                    break;
                default:
                    msg = 'Lỗi định vị. Hãy gõ địa chỉ hoặc click trên bản đồ.';
            }
            console.log('Geolocation error:', msg);
            
            // Show tooltip instead of alert
            btn.title = msg;
            btn.textContent = '⚠️ ' + msg.split('.')[0];
            setTimeout(function() {
                btn.textContent = originalText;
                btn.title = '';
            }, 3000);
        },
        {
            enableHighAccuracy: false,
            timeout: 10000,
            maximumAge: 300000
        }
    );
}
