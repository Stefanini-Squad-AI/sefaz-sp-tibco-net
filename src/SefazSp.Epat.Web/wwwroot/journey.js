// bpmn-js interop for the ePAT live viewer (navigated-viewer: pan+zoom, no editing).
// Uses the overlays API for step badges, graduated colors, and current-node callouts.
window.epatJourney = (function () {
    var viewer = null;
    var sourceXml = null;

    // Node-ID → human label (from Sc001NodePath + branch tails).
    var nodeNames = {
        '_OAgPol9UEfG6Lfb98zsREQ': 'Iniciar Novo Graft',
        '_XWivF1qTEfG5K7mY0I3I6w': 'Set Nome Etapa 2',
        '_sfwu-VqUEfG5K7mY0I3I6w': 'Preparar Notificação',
        '_sJqYklqTEfG5K7mY0I3I6w': 'Corrigir?',
        '_tN6q4lqTEfG5K7mY0I3I6w': 'Corrigir Fechamento',
        '_5E444FqTEfG5K7mY0I3I6w': 'Corrigir Fechamento',
        '_xWNLe1qSEfG5K7mY0I3I6w': 'Finalizar AIIM',
        '_Faq_RFqTEfG5K7mY0I3I6w': 'Execução Paralela',
        '_IxqJMlqTEfG5K7mY0I3I6w': 'Existe Notificação?',
        '_Faq_RVqTEfG5K7mY0I3I6w': 'Inicia Graft Step',
        '_0XWagFqNEfG5K7mY0I3I6w': 'Inicia Graft Step',
        '_0XWahVqNEfG5K7mY0I3I6w': 'Flag Retirati',
        '_0XWagVqNEfG5K7mY0I3I6w': 'DEAT0050',
        '_0XWahFqNEfG5K7mY0I3I6w': 'Trocar Notificação?',
        '_LeuhgFqVEfG5K7mY0I3I6w': 'Iniciar Decisions',
        '_CI6l0VqREfG5K7mY0I3I6w': 'Iniciar Decisions',
        '_CI6lx1qREfG5K7mY0I3I6w': 'Verificar Anulação',
        '_CI6lyFqREfG5K7mY0I3I6w': 'PRPINTPC',
        '_G4hU81qhEfG5K7mY0I3I6w': 'Define Destinatários',
        '_6WNq-lqgEfG5K7mY0I3I6w': 'Email Limite Rel 1',
        '_30jAcFqVEfG5K7mY0I3I6w': 'Verificar Retorno',
        '_89MVQlqVEfG5K7mY0I3I6w': 'Validar Paralelos',
        '_Ei94AFqPEfG5K7mY0I3I6w': 'Validação Paralelos',
        '_CtQ7BFqPEfG5K7mY0I3I6w': 'Vistas do Juiz?',
        '_CtQ6-1qPEfG5K7mY0I3I6w': 'Vistas do Juiz',
        '_CtQ6_VqPEfG5K7mY0I3I6w': 'Vista Mista?',
        '_CtQ6-lqPEfG5K7mY0I3I6w': 'Convergência',
        '_zE3XeV6JEfGBBLgT-R5iuw': 'prepSub',
        '_nQntZ16JEfGBBLgT-R5iuw': 'Controlar Intimados',
        '_H22mclqWEfG5K7mY0I3I6w': 'Fim',
        '_CtQ7BVqPEfG5K7mY0I3I6w': 'Convergência',
        '_tbOD4FqPEfG5K7mY0I3I6w': 'VistaAgenda',
        '_InbWgFqQEfG5K7mY0I3I6w': 'Fim Vista Mista',
        '_CtQ67FqPEfG5K7mY0I3I6w': 'Mista',
        '_CtQ66lqPEfG5K7mY0I3I6w': 'Validar Paralelos',
        '_CtQ68lqPEfG5K7mY0I3I6w': 'Pedido de Vistas',
        '_CtQ7A1qPEfG5K7mY0I3I6w': 'Fim de Prazo',
        '_CtQ66FqPEfG5K7mY0I3I6w': 'FimDRF',
        '_WvTQIFqQEfG5K7mY0I3I6w': 'Busca Envolvidos',
        '_Xw86YlqQEfG5K7mY0I3I6w': 'Fim',
        '_Faq_Q1qTEfG5K7mY0I3I6w': 'Fim (SC-014)',
        '_BQIgAF9KEfGqPfX31TKC3w': 'Criar Notificação',
        '_O7K3MF9LEfGqPfX31TKC3w': 'Fim (SC-015)'
    };

    function greenForIndex(i, total) {
        var t = total <= 1 ? 1 : i / (total - 1);
        var lightness = Math.round(85 - 40 * t);
        return 'hsl(142, 60%, ' + lightness + '%)';
    }

    return {
        render: async function (elementId, xml) {
            if (viewer) { try { viewer.destroy(); } catch (e) { } viewer = null; }
            sourceXml = xml;
            // Disable selection visuals (the module that draws thick outlines)
            var noopSelectionVisuals = {
                __init__: ['selectionVisuals'],
                selectionVisuals: ['type', function () {}]
            };
            viewer = new window.BpmnJS({
                container: '#' + elementId,
                additionalModules: [noopSelectionVisuals]
            });
            try {
                await viewer.importXML(xml);
                var canvas = viewer.get('canvas');
                canvas.zoom('fit-viewport');
                // Suppress hover highlight
                var eventBus = viewer.get('eventBus');
                eventBus.on('element.hover', 10000, function (e) { e.stopPropagation(); });
                eventBus.on('element.out', 10000, function (e) { e.stopPropagation(); });
            } catch (e) {
                console.error('bpmn importXML failed', e);
            }
        },

        apply: function (traversed, current) {
            if (!viewer) return;
            var overlays = viewer.get('overlays');
            var elementRegistry = viewer.get('elementRegistry');
            var canvas = viewer.get('canvas');
            var total = (traversed || []).length;

            // Clear previous overlays and markers
            try { overlays.remove({ type: 'step-badge' }); } catch (e) { }
            try { overlays.remove({ type: 'step-color' }); } catch (e) { }
            try { overlays.remove({ type: 'current-callout' }); } catch (e) { }
            elementRegistry.forEach(function (el) {
                try { canvas.removeMarker(el.id, 'current'); } catch (e) { }
            });

            (traversed || []).forEach(function (id, i) {
                var shape = elementRegistry.get(id);
                if (!shape) return;
                var w = shape.width || 36;
                var h = shape.height || 36;

                // Graduated color overlay
                try {
                    var colorDiv = document.createElement('div');
                    colorDiv.className = 'step-color-overlay';
                    colorDiv.style.width = w + 'px';
                    colorDiv.style.height = h + 'px';
                    colorDiv.style.background = greenForIndex(i, total);
                    overlays.add(id, 'step-color', {
                        position: { top: 0, left: 0 },
                        html: colorDiv
                    });
                } catch (e) { }

                // Step number badge
                try {
                    overlays.add(id, 'step-badge', {
                        position: { top: -10, left: -10 },
                        html: '<div class="step-badge">' + (i + 1) + '</div>'
                    });
                } catch (e) { }
            });

            // Current node: pulsing callout with human name
            if (current) {
                var name = nodeNames[current] || current;
                try {
                    var shape = elementRegistry.get(current);
                    var w = shape ? (shape.width || 80) : 80;
                    overlays.add(current, 'current-callout', {
                        position: { bottom: -6, left: Math.round(w / 2) - 60 },
                        html: '<div class="current-callout">\u23f8 ' + name + '</div>'
                    });
                    canvas.addMarker(current, 'current');
                } catch (e) { }
            }
        },

        zoom: function (delta) {
            if (!viewer) return;
            var c = viewer.get('canvas');
            c.zoom(Math.min(4, Math.max(0.2, c.zoom() + delta)));
        },

        fit: function () {
            if (!viewer) return;
            viewer.get('canvas').zoom('fit-viewport');
        },

        reset: async function () {
            if (!viewer || !sourceXml) return;
            await viewer.importXML(sourceXml);
            viewer.get('canvas').zoom('fit-viewport');
        }
    };
})();
