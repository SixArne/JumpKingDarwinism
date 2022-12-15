import DefaultTheme from 'vitepress/theme'
import VueClickAwayExample from './components/VueClickAwayExample.vue'
import CustomProjectCards from './components/CustomProjectCards.vue'
import CustomModelProjector from './components/CustomModelProjector.vue'

// Custom css
import './custom.css'

export default {
  ...DefaultTheme,
  enhanceApp(ctx) {
    DefaultTheme.enhanceApp(ctx)
    ctx.app.component('VueClickAwayExample', VueClickAwayExample)
    ctx.app.component('CustomProjectCards', CustomProjectCards)
    ctx.app.component('CustomModelProjector', CustomModelProjector)
  }
}