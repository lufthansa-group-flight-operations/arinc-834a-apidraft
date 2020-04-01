//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import Foundation

// The ViewRouter is a global provider of which menu item is selected.
// This is needed for the ContentView to know which view should be presented.
class ViewRouter: ObservableObject {
    @Published var activeView = "home"
}
