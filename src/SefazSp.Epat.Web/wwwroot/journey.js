// bpmn-js interop for the ePAT live viewer. The CDN script exposes window.BpmnJS
// (bpmn-navigated-viewer). C# only ever deals in node ids.
window.epatJourney = (function () {
    let viewer = null;

    return {
        render: async function (elementId, xml) {
            if (viewer) { try { viewer.destroy(); } catch (e) { } viewer = null; }
            viewer = new window.BpmnJS({ container: '#' + elementId });
            try {
                await viewer.importXML(xml);
                viewer.get('canvas').zoom('fit-viewport');
            } catch (e) {
                console.error('bpmn importXML failed', e);
            }
        },

        // traversed: string[] of node ids; current: string|null.
        apply: function (traversed, current) {
            if (!viewer) return;
            const canvas = viewer.get('canvas');
            (traversed || []).forEach(function (id) {
                try {
                    canvas.removeMarker(id, 'current');
                    canvas.addMarker(id, 'traversed');
                } catch (e) { /* node id not in this diagram */ }
            });
            if (current) {
                try { canvas.addMarker(current, 'current'); } catch (e) { }
            }
        }
    };
})();
