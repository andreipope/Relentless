from base import CZBTests
import time
class Deck_Selection_Page(CZBTests):

    def __init__(self,altdriver):
        self.altdriver=altdriver
        self.deck_selection_page=self.deck_mode_selection_page()
        self.left_arrow_button=self.get_left_arrow_button()
        self.right_arrow_button=self.get_right_arrow_button()
        self.deck_container_panel=self.get_deck_container_panel()
        self.edit_button=self.get_edit_button()
        self.delete_button=self.get_delete_button()
        self.rename_button=self.get_rename_button()
        self.filter_button=self.get_filter_button()
        self.search_deck_input_field=self.get_search_deck_input_field()
        
    
    def deck_mode_selection_page(self):
        return self.altdriver.wait_for_element('MyDecksPage(Clone)/Tab_SelectDeck')
    def get_left_arrow_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Panel_Content/Button_LeftArrow')
    def get_right_arrow_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Panel_Content/Button_RightArrow')
    def get_deck_container_panel(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Panel_Content')
    def get_edit_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Panel_FrameComponents/Lower_Items/Button_Edit')
    def get_delete_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Panel_FrameComponents/Lower_Items/Button_Delete')
    def get_rename_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Panel_FrameComponents/Lower_Items/Button_Rename')
    def get_filter_button(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Panel_FrameComponents/Upper_Items/Button_Filter')
    def get_search_deck_input_field(self):
        return self.altdriver.wait_for_element(self.deck_selection_page.name+'/Panel_FrameComponents/Upper_Items/InputText_SearchDeckName')

    def start_match(self):
        self.button_pressed(self.battle_button)
    def go_to_army_page(self):
        self.button_pressed(self.army_button)
    def go_back_to_menu(self):
        self.button_pressed(self.back_button)
    
    def press_right_arrow(self):
        self.button_pressed(self.right_arrow_button)
        time.sleep(1)

    def press_left_arrow(self):
        self.button_pressed(self.left_arrow_button)
        time.sleep(1)
    def press_edit_button(self):
        self.button_pressed(self.edit_button)
    def press_delete_button(self):
        self.button_pressed(self.delete_button)

    def get_selected_deck(self):
        decks_present=self.altdriver.find_elements(self.deck_container_panel.name+'/Group/Item_HordeSelectionObject(Clone)/Button_Select',enabled=False)
        for button in decks_present:
            print('button enabled '+button.enabled)
            if button.enabled=='False':
                return self.altdriver.wait_for_element('id('+button.id+')/..',enabled=False)
        return None

    def get_deck_name(self,deck):
        print(deck.name+"  "+ deck.id)
        text_element=self.altdriver.find_element('id('+deck.id+')/Text_DeckName')
        return self.read_tmp_UGUI_text(text_element)
    def get_deck_number_of_cards(self,deck):
        text_element=self.altdriver.find_element('id('+deck.id+')/Text_CardsCount')
        return self.read_tmp_UGUI_text(text_element).split('/')[0]

    # def select_last_deck(self):
    #     selected_deck=self.get_selected_deck()
    #     while True:
    #         self.press_right_arrow()
    #         new_selected_deck=self.get_selected_deck()
    #         if selected_deck.id==new_selected_deck.id:
    #             break
    #         selected_deck=new_selected_deck
    # def select_first_deck(self):
    #     selected_deck=self.get_selected_deck()
    #     while True:
    #         self.press_left_arrow()
    #         new_selected_deck=self.get_selected_deck()
    #         if selected_deck.id==new_selected_deck.id:
    #             break
    #         selected_deck=new_selected_deck

    
    def get_current_page_number(self):
        pageCircles=self.altdriver.find_elements_where_name_contains('Image_CircleDot_')
        for index in range(len(pageCircles)):
            if pageCircles[index].name=='Image_CircleDot_Selected(Clone)':
                return index
        return None
    def get_total_number_of_pages(self):
        return len(pageCircles=self.altdriver.find_elements_where_name_contains('Image_CircleDot_'))
    def select_page(self,page_number):
        if page_number>self.get_current_page_number():
            for index in range(page_number-self.get_current_page_number):
                self.press_left_arrow()
        else:
             for index in range(self.get_current_page_number-page_number):
                self.press_right_arrow()
    def get_decks_shown_in_page(self):
        return self.altdriver.find_elements_where_name_contains('Image_DeckThumbnailNormal')
    def select_deck(self,deck_name):
        select_page(0)
        for index in range(get_total_number_of_pages()):
            decks=get_decks_shown_in_page()
            for deck in decks:
                if deck_name==get_deck_name(deck):
                    return deck
            self.press_right_arrow()
        return None
        

    def create_new_deck(self):
        # self.select_first_deck()
        self.altdriver.find_element('Panel_Content/Button_BuildNewDeck').mobile_tap()
    # def create_new_deck_tutorial(self):
    #     self.select_last_deck()
    #     self.altdriver.find_element('Item_HordeSelectionNewHorde/Image_BaackgroundGeneral').mobile_tap()



