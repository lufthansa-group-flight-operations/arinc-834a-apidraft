//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import Foundation

// A simple representation of AvionicParameters with an id for later identification
struct AvionicParameter: Identifiable {
    var id = UUID()
    var name: String
    var value: String
    var timestamp: String
    var state: String
}
