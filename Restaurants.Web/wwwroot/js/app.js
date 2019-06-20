function initMap() {
    var app = new Vue({
        el: "#app",
        data() {
            return {
                map: "",
                places: "",
                infoWindow: "",
                markers: [],
                autocomplete: "",
                MARKER_PATH: 'https://developers.google.com/maps/documentation/javascript/images/marker_green',
                hostnameRegexp: new RegExp('^https?://.+?/'),
                response: [],
                Result: [],
                keyword: 'Bangsue',
                isViewReady: false
            };
        },
        mounted() {
            this.initMap();
        },
        methods: {
            initMap: function () {
                this.map = new google.maps.Map(document.getElementById('map'), {
                    zoom: 13,
                    center: { lat: 13.736717, lng: 100.523186 },
                    mapTypeControl: false,
                    panControl: false,
                    zoomControl: false,
                    streetViewControl: false
                });

                this.infoWindow = new google.maps.InfoWindow({
                    content: document.getElementById('info-content')
                });

                this.places = new google.maps.places.PlacesService(this.map);
            },

            refreshData: function () {
                var self = this;
                this.isViewReady = false;

                axios.get('/api/subscribers/getplace/', {
                    params: {
                        keyword: this.keyword
                    }
                })
                    .then(function (response) {
                        self.response = response.data;
                        self.clearResults();
                        self.clearMarkers();
                        self.map.panTo(self.response.results[0].geometry.location);
                        self.map.setZoom(15);
                        self.createMarker();
                    })
                    .catch(function (error) {
                        alert("ERROR: " + (error.message | error));
                    });
            },

            createMarker() {
                var self = this;
                // Create a marker for each restaurant found, and
                // assign a letter of the alphabetic to each marker icon.
                for (var i = 0; i < self.response.results.length; i++) {
                    var markerLetter = String.fromCharCode('A'.charCodeAt(0) + (i % 26));
                    var markerIcon = self.MARKER_PATH + markerLetter + '.png';
                    // Use marker animation to drop the icons incrementally on the map.
                    self.markers[i] = new google.maps.Marker({
                        position: self.response.results[i].geometry.location,
                        animation: google.maps.Animation.DROP,
                        icon: markerIcon
                    });
                    // If the user clicks a restaurant marker, show the details of that restaurant
                    // in an info window.
                    self.markers[i].placeResult = self.response.results[i];
                                
                    google.maps.event.addListener(self.markers[i], 'click', (function (marker, i) {
                        return function () {
                            self.places.getDetails({ placeId: self.markers[i].placeResult.place_id },
                                function (place, status) {
                                    if (status !== google.maps.places.PlacesServiceStatus.OK) {
                                        return;
                                    }
                                    self.infoWindow.open(this.map, self.markers[i]);
                                    self.buildIWContent(place);
                                    self.map.setZoom(18);
                                    self.map.setCenter(self.markers[i].getPosition());
                                });
                        }
                    })(self.markers[i], i));

                    setTimeout(self.dropMarker(i), i * 100);
                    self.addResult(self.response.results[i], i);
                }
            },

            clearMarkers() {
                var self = this;
                for (var i = 0; i < self.markers.length; i++) {
                    if (self.markers[i]) {
                        self.markers[i].setMap(null);
                    }
                }
                self.markers = [];
            },

            dropMarker(i) {
                var self = this;
                return function () {
                    self.markers[i].setMap(self.map);
                };
            },

            addResult(result, i) {
                var self = this;
                var results = document.getElementById('results');
                var markerLetter = String.fromCharCode('A'.charCodeAt(0) + (i % 26));
                var markerIcon = this.MARKER_PATH + markerLetter + '.png';

                var tr = document.createElement('tr');
                tr.style.backgroundColor = (i % 2 === 0 ? '#F0F0F0' : '#FFFFFF');
                tr.onclick = function () {
                    google.maps.event.trigger(self.markers[i], 'click');
                };

                var iconTd = document.createElement('td');
                var nameTd = document.createElement('td');
                var icon = document.createElement('img');
                icon.src = markerIcon;
                icon.setAttribute('class', 'placeIcon');
                icon.setAttribute('className', 'placeIcon');
                var name = document.createTextNode(result.name);
                iconTd.appendChild(icon);
                nameTd.appendChild(name);
                tr.appendChild(iconTd);
                tr.appendChild(nameTd);
                results.appendChild(tr);
            },

            clearResults() {
                var results = document.getElementById('results');
                while (results.childNodes[0]) {
                    results.removeChild(results.childNodes[0]);
                }
            },

            // Get the place details for a restaurant. Show the information in an info window,
            // anchored on the marker for the restaurant that the user selected.
            showInfoWindow() {
                var marker = this;
                this.places.getDetails({ placeId: marker.placeResult.place_id },
                    function (place, status) {
                        if (status !== google.maps.places.PlacesServiceStatus.OK) {
                            return;
                        }
                        this.infoWindow.open(this.map, self.marker);
                        this.buildIWContent(place);
                        this.map.setZoom(18);
                        this.map.setCenter(self.marker.getPosition());
                    });
            },

            // Load the place information into the HTML elements used by the info window.
            buildIWContent(place) {
                document.getElementById('iw-icon').innerHTML = '<img class="Icon" ' +
                    'src="' + place.icon + '"/>';
                document.getElementById('iw-url').innerHTML = '<b><a href="' + place.url +
                    '">' + place.name + '</a></b>';
                document.getElementById('iw-address').textContent = place.vicinity;

                if (place.formatted_phone_number) {
                    document.getElementById('iw-phone-row').style.display = '';
                    document.getElementById('iw-phone').textContent =
                        place.formatted_phone_number;
                } else {
                    document.getElementById('iw-phone-row').style.display = 'none';
                }

                // Assign a five-star rating to the restaurant, using a black star ('&#10029;')
                // to indicate the rating the restaurant has earned, and a white star ('&#10025;')
                // for the rating points not achieved.
                if (place.rating) {
                    var ratingHtml = '';
                    for (var i = 0; i < 5; i++) {
                        if (place.rating < (i + 0.5)) {
                            ratingHtml += '&#10025;';
                        } else {
                            ratingHtml += '&#10029;';
                        }
                        document.getElementById('iw-rating-row').style.display = '';
                        document.getElementById('iw-rating').innerHTML = ratingHtml;
                    }
                } else {
                    document.getElementById('iw-rating-row').style.display = 'none';
                }

                // The regexp isolates the first part of the URL (domain plus subdomain)
                // to give a short URL for displaying in the info window.
                if (place.website) {
                    var fullUrl = place.website;
                    var website = this.hostnameRegexp.exec(place.website);
                    if (website === null) {
                        website = 'http://' + place.website + '/';
                        fullUrl = website;
                    }
                    document.getElementById('iw-website-row').style.display = '';
                    document.getElementById('iw-website').textContent = website;
                } else {
                    document.getElementById('iw-website-row').style.display = 'none';
                }
            }
        },
        created: function () {
            this.refreshData();
        }
    });
}